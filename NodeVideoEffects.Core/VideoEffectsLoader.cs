// #define ASM_EXPORT

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using NodeVideoEffects.Utility;
using SharpGen.Runtime;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.ItemEditor;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Effects;

namespace NodeVideoEffects.Core;

public delegate void MapInputRectsToOutputRectDelegate(
    RawRect[] inputRects,
    RawRect[] inputOpaqueSubRects,
    out RawRect outputRect,
    out RawRect outputOpaqueSubRect);

public delegate void MapOutputRectToInputRectsDelegate(RawRect outputRect, RawRect[] inputRects);

public class VideoEffectsLoader : IDisposable
{
    private static readonly Dictionary<string, byte[]> ShaderDictionaries = [];
    private readonly string _id;
    private readonly ShaderEffect? _shaderEffect;
    private readonly EffectType _type;
    private readonly IVideoEffect? _videoEffect;
    private IVideoEffectProcessor? _processor;

    private VideoEffectsLoader(IVideoEffect? effect, string id)
    {
        _videoEffect = effect ?? throw new ArgumentNullException(nameof(effect), @"Unable load effect");
        _type = EffectType.VideoEffect;
        _id = id;
    }

    private VideoEffectsLoader(ShaderEffect? effect, string id)
    {
        if (effect == null) throw new ArgumentNullException(nameof(effect), @"Unable generate effect");
        _shaderEffect = effect;
        _type = EffectType.ShaderEffect;
        _id = id;
    }

    public void Dispose()
    {
        _processor?.ClearInput();
        _processor?.Dispose();
        _processor = null;
        GC.SuppressFinalize(this);
    }

    ~VideoEffectsLoader()
    {
        Dispose();
    }

    public VideoEffectsLoader SetValue(params object[]? values)
    {
        if (values == null) return this;
        switch (_type)
        {
            case EffectType.VideoEffect when _videoEffect != null:
            {
                return this;
            }
            case EffectType.ShaderEffect when _shaderEffect != null:
            {
                lock (_shaderEffect)
                {
                    for (var i = 0; i < values.Length; i++) _shaderEffect.SetValueByIndex(i, values[i]);
                }

                return this;
            }

            default:
                return this;
        }
    }

    public async Task<VideoEffectsLoader> SetValue(string propertyName, object? value)
    {
        switch (_type)
        {
            case EffectType.ShaderEffect when _shaderEffect != null:
                lock (_shaderEffect)
                {
                    _shaderEffect.SetValueByName(propertyName, value);
                }

                break;
            case EffectType.VideoEffect when _videoEffect != null:
            {
                // Recursively search for properties with DisplayAttribute within the _videoEffect hierarchy,
                // and match the property name (identifier) with the argument propertyName
                var result = FindPropertyByDisplay(_videoEffect, propertyName);
                if (result == null)
                    throw new ArgumentException($@"The specified property '{propertyName}' was not found.",
                        nameof(propertyName));

                var (targetObject, propInfo) = result.Value;

                // If the property type is Animation, update the Values property of the Animation object directly
                if (propInfo.PropertyType == typeof(Animation))
                {
                    // Retrieve the Animation object. If it does not exist, create a new one
                    if (propInfo.GetValue(targetObject) is not Animation animObj)
                    {
                        if (!propInfo.CanWrite)
                            throw new InvalidOperationException($"The property '{propertyName}' is read-only.");
                        animObj = Activator.CreateInstance<Animation>()
                                  ?? throw new InvalidOperationException(
                                      "Unable to create an instance of Animation type.");
                        // Set the Animation object to the target property only if it did not exist
                        propInfo.SetValue(targetObject, animObj);
                    }

                    // Retrieve the Values property of the Animation object
                    var valuesProp = animObj.GetType()
                        .GetProperty("Values", BindingFlags.Public | BindingFlags.Instance);
                    if (valuesProp == null || !valuesProp.CanRead || !valuesProp.CanWrite)
                        throw new InvalidOperationException(
                            "The Values property of the Animation object was not found or is read-only.");

                    // Create a new AnimationValue and add it to the existing list
                    var newList =
                        ImmutableList<AnimationValue>.Empty.Add(new AnimationValue(Convert.ToDouble(value ?? 0)));
                    // Update the Values property of the Animation object directly
                    animObj.BeginEdit();
                    valuesProp.SetValue(animObj, newList);
                    await animObj.EndEditAsync();
                }
                else
                {
                    if (!propInfo.CanWrite)
                        throw new InvalidOperationException($"The property '{propertyName}' is read-only.");
                    // For non-Animation types, set the value to the property as usual
                    propInfo.SetValue(targetObject, value);
                }

                if (_videoEffect is IEditable editable) await editable.EndEditAsync();
            }
                break;
        }

        return this;

        (object target, PropertyInfo property)? FindPropertyByDisplay(object? obj, string name)
        {
            if (obj == null) return null;

            // Traverse the properties directly under obj
            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Exclude properties that require parameters, such as indexers
                if (prop.GetIndexParameters().Length > 0)
                    continue;

                // Check if DisplayAttribute is applied and compare the property name
                var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                if (displayAttr != null && prop.Name == name) return (obj, prop);

                // Recursively search (excluding string type)
                if (prop is not { CanRead: true, PropertyType.IsClass: true } ||
                    prop.PropertyType == typeof(string)) continue;
                object? subObj;
                try
                {
                    subObj = prop.GetValue(obj);
                }
                catch
                {
                    // Skip if an exception occurs while retrieving the property
                    continue;
                }

                if (subObj == null) continue;
                var result = FindPropertyByDisplay(subObj, name);
                if (result != null)
                    return result;
            }

            return null;
        }
    }

    public bool Update(ID2D1Image? image, out ID2D1Image? output)
    {
        output = null;
        switch (_type)
        {
            case EffectType.VideoEffect when _videoEffect != null:
            {
                _processor ??= _videoEffect.CreateVideoEffect(NodesManager.GetContext(_id));
                if (image == null) return false;
                lock (_processor)
                {
                    try
                    {
                        if (_processor.Output.NativePointer == IntPtr.Zero)
                            _processor = _videoEffect.CreateVideoEffect(NodesManager.GetContext(_id));
                        _processor.SetInput(image);
                        _processor.Update(NodesManager.GetInfo(_id));
                        output = _processor.Output;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            case EffectType.ShaderEffect when _shaderEffect != null:
            {
                lock (_shaderEffect)
                {
                    try
                    {
                        _shaderEffect.SetInput(0, image, true);
                        output = _shaderEffect.Output;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            default:
                return false;
        }
    }

    public VideoEffectsLoader SetInputImageMargin(Rect margin)
    {
        if (_type != EffectType.ShaderEffect || _shaderEffect == null) return this;
        lock (_shaderEffect)
        {
            _shaderEffect?.SetInputImageMargin(margin);
        }

        return this;
    }

    public VideoEffectsLoader SetOutputImageMargin(Rect margin)
    {
        if (_type != EffectType.ShaderEffect || _shaderEffect == null) return this;
        lock (_shaderEffect)
        {
            _shaderEffect?.SetOutputImageMargin(margin);
        }

        return this;
    }

    public static async Task<VideoEffectsLoader> LoadEffect(string name, string id)
    {
        return await Task.Run(() => LoadEffectSync(name, id));
    }

    public static VideoEffectsLoader LoadEffectSync(string name, string id)
    {
        return new VideoEffectsLoader(
            Activator.CreateInstance(PluginLoader.VideoEffects.ToList().First(type => type.Name == name)) as
                IVideoEffect, id);
    }

    public static async Task<VideoEffectsLoader> LoadEffect(List<(Type type, string name)> properties,
        string shaderResourceId, string effectId)
    {
        return await Task.Run(() => LoadEffectSync(properties, shaderResourceId, effectId));
    }

    public static VideoEffectsLoader LoadEffectSync(List<(Type type, string name)> properties,
        string shaderResourceId, string effectId)
    {
        if (shaderResourceId == "")
            throw new ArgumentException("Shader resource id is empty.");
        var effect = ShaderEffect.Create(effectId, properties, shaderResourceId);
        if (effect.IsEnabled) return new VideoEffectsLoader(effect, effectId);
        effect.Dispose();
        effect = null;
        return new VideoEffectsLoader(effect, effectId);
    }

    public static string RegisterShader(string shaderName)
    {
        byte[] shader;
        var asm = Assembly.GetCallingAssembly();
        var resName = asm.GetManifestResourceNames().FirstOrDefault(a => a.EndsWith(shaderName), "");

        if (resName == "")
        {
            Logger.Write(LogLevel.Error, $"The shader resource \"*.{shaderName}\" not found.");
            return "";
        }

        using (var resourceStream = asm.GetManifestResourceStream(resName))
        {
            if (resourceStream == null)
            {
                Logger.Write(LogLevel.Error, $"The shader resource \"{resName}\" not found.");
                return "";
            }

            using (var memoryStream = new MemoryStream())
            {
                resourceStream.CopyTo(memoryStream);
                shader = memoryStream.ToArray();
            }
        }

        var id = Guid.NewGuid().ToString("N");
        ShaderDictionaries.TryAdd(id, shader);
        return id;
    }

    public static byte[] GetShader(string id)
    {
        return ShaderDictionaries[id];
    }

    private enum EffectType
    {
        VideoEffect,
        ShaderEffect
    }

    public abstract class ShaderEffect : D2D1CustomShaderEffectBase
    {
        private int _propertiesCount;

        public ShaderEffect(nint ptr) : base(ptr)
        {
        }

        public static ShaderEffect Create(string effectId, List<(Type type, string name)> properties, string shaderId)
        {
            // Generate a unique class name based on properties
            var className = $"ShaderEffect_{shaderId}_{string.Join("_", properties.Select(p => p.type.Name + p.name))}";
            var effectType = GenerateEffectType(className, properties, shaderId);

            var context = NodesManager.GetContext(effectId);
            var effectInstance = Activator.CreateInstance(effectType, context) as ShaderEffect
                                 ?? throw new InvalidOperationException("Cannot create effect instance");

            effectInstance._propertiesCount = properties.Count;

            return effectInstance
                   ?? throw new InvalidOperationException("Cannot cast to ShaderEffect");
        }

        public void SetValueByIndex(int index, object? value)
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (index < 0 || index >= properties.Length)
                throw new IndexOutOfRangeException("The specified index is out of range.");

            var property = properties[index];

            if (!property.CanWrite)
                throw new InvalidOperationException($"The property '{property.Name}' is not writable.");

            property.SetValue(this, value);
        }

        public void SetInputImageMargin(Rect margin)
        {
            SetValue(_propertiesCount + 0, (int)margin.Left);
            SetValue(_propertiesCount + 1, (int)margin.Top);
            SetValue(_propertiesCount + 2, (int)margin.Right);
            SetValue(_propertiesCount + 3, (int)margin.Bottom);
        }

        public void SetOutputImageMargin(Rect margin)
        {
            SetValue(_propertiesCount + 4, (int)margin.Left);
            SetValue(_propertiesCount + 5, (int)margin.Top);
            SetValue(_propertiesCount + 6, (int)margin.Right);
            SetValue(_propertiesCount + 7, (int)margin.Bottom);
        }

        public void SetValueByName(string propertyName, object? value)
        {
            // 指定されたプロパティ名（名前文字列）に対応するプロパティを取得する（BindingFlags: 公開・インスタンス）
            var property = GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                throw new ArgumentException($@"指定されたプロパティ '{propertyName}' が見つかりません。", nameof(propertyName));
            if (!property.CanWrite) throw new InvalidOperationException($"プロパティ '{propertyName}' は書き込み不可です。");
            property.SetValue(this, value);
        }

        private static Type GenerateEffectType(string className, List<(Type, string)> properties, string shaderId)
        {
            AssemblyName assemblyName = new("DynamicID2D1PropertiesAssembly");
#if ASM_EXPORT
            var persistedAssemblyBuilder = new PersistedAssemblyBuilder(assemblyName, typeof(object).Assembly);
            var moduleBuilder = persistedAssemblyBuilder.DefineDynamicModule("MainModule");
#else
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
#endif

            var typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public, typeof(ShaderEffect));

            var effectImplType = DynamicEffectImplGenerator.GenerateEffectImpl(properties, shaderId, moduleBuilder);

            var index = 0;
            foreach (var (type, name) in properties)
            {
                //
                // Define getter
                // {type} get_{name}()
                // {
                //     return GetValue<{type}>({index});
                //     // e.g. if type is float:
                //     //    return GetFloatValue(index);
                //     // * The method info is from MakeGetterMethodInfo().
                // }
                //
                var getterBuilder = typeBuilder.DefineMethod(
                    $"get_{name}",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    type,
                    Type.EmptyTypes
                );

                // Get the method info for the getter
                var getterInfo = MakeGetterMethodInfo(type) ??
                                 throw new InvalidOperationException("Cannot get the method \"GetValue\"");

                var getterIl = getterBuilder.GetILGenerator();
                // return GetValue<{type}>({index});
                getterIl.Emit(OpCodes.Ldarg_0);
                getterIl.Emit(OpCodes.Ldc_I4, index);
                getterIl.Emit(OpCodes.Call, getterInfo);
                getterIl.Emit(OpCodes.Ret);

                //
                // Define setter
                // set_{name}({type} value)
                // {
                //    SetValue({index}, value);
                //    return;
                // }
                //
                var setterBuilder = typeBuilder.DefineMethod(
                    $"set_{name}",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    null,
                    [type]
                );

                var setterIl = setterBuilder.GetILGenerator();
                // SetValue(index, value);
                setterIl.Emit(OpCodes.Ldarg_0);
                setterIl.Emit(OpCodes.Ldc_I4, index);
                setterIl.Emit(OpCodes.Ldarg_1);
                setterIl.Emit(OpCodes.Call, typeof(ID2D1Properties).GetMethod("SetValue", [typeof(int), type])
                                            ?? throw new InvalidOperationException(
                                                "Cannot get the method \"SetValue\""));
                getterIl.Emit(OpCodes.Nop);
                setterIl.Emit(OpCodes.Ret);

                // Define the property
                // public {type} {name} { get => get_{name}(); set => set_{name}(value); }
                var propertyBuilder = typeBuilder.DefineProperty(
                    name,
                    PropertyAttributes.None,
                    type,
                    null
                );

                propertyBuilder.SetGetMethod(getterBuilder);
                propertyBuilder.SetSetMethod(setterBuilder);

                index++;
            }

            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                [typeof(IGraphicsDevicesAndContext)]);
            var ctorIl = ctorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldarg_1);
            ctorIl.Emit(OpCodes.Call, typeof(D2D1CustomShaderEffectBase)
                                          .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                                          .FirstOrDefault(m => m is { Name: "Create", IsGenericMethod: true })
                                          ?.MakeGenericMethod(effectImplType)
                                      ?? throw new InvalidOperationException("Cannot get Create method"));
            ctorIl.Emit(OpCodes.Call, typeof(ShaderEffect).GetConstructor([typeof(nint)])
                                      ?? throw new InvalidOperationException("Cannot get the constructor"));
            ctorIl.Emit(OpCodes.Nop);
            ctorIl.Emit(OpCodes.Ret);

            var generateEffectType = typeBuilder.CreateTypeInfo().AsType();
#if ASM_EXPORT
            persistedAssemblyBuilder.Save("EffectModule.dll");
#endif
            return generateEffectType
                   ?? throw new InvalidOperationException("Cannot create the type");

            MethodInfo? MakeGetterMethodInfo(Type type)
            {
                return type switch
                {
                    not null when type == typeof(bool) => typeof(ShaderEffect).GetMethod("GetBoolValue"),
                    not null when type == typeof(Guid) => typeof(ShaderEffect).GetMethod("GetGuidValue"),
                    not null when type == typeof(float) => typeof(ShaderEffect).GetMethod("GetFloatValue"),
                    not null when type == typeof(int) => typeof(ShaderEffect).GetMethod("GetIntValue"),
                    not null when type == typeof(Matrix3x2) => typeof(ShaderEffect).GetMethod("GetMatrix3x2Value"),
                    not null when type == typeof(Matrix4x3) => typeof(ShaderEffect).GetMethod("GetMatrix4x3Value"),
                    not null when type == typeof(Matrix4x4) => typeof(ShaderEffect).GetMethod("GetMatrix4x4Value"),
                    not null when type == typeof(Matrix5x4) => typeof(ShaderEffect).GetMethod("GetMatrix5x4Value"),
                    not null when type == typeof(string) => typeof(ShaderEffect).GetMethod("GetStringValue"),
                    not null when type == typeof(uint) => typeof(ShaderEffect).GetMethod("GetUIntValue"),
                    not null when type == typeof(Vector2) => typeof(ShaderEffect).GetMethod("GetVector2Value"),
                    not null when type == typeof(Vector3) => typeof(ShaderEffect).GetMethod("GetVector3Value"),
                    not null when type == typeof(Vector4) => typeof(ShaderEffect).GetMethod("GetVector4Value"),
                    not null when type == typeof(Enum) => typeof(ShaderEffect).GetMethod("GetEnumValue")
                        ?.MakeGenericMethod(type),
                    not null when type == typeof(ComObject) => typeof(ShaderEffect).GetMethod("GetIUnknownValue")
                        ?.MakeGenericMethod(type),
                    not null when type == typeof(ID2D1ColorContext) => typeof(ShaderEffect).GetMethod(
                        "GetColorContextValue"),
                    _ => throw new InvalidOperationException("Unsupported type")
                };
            }
        }
    }
}