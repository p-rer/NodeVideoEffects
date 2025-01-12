using System.Reflection;
using System.Reflection.Emit;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Effects;
using SharpGen.Runtime;
using System.Numerics;
using Vortice.Mathematics;
using NodeVideoEffects.Utility;
using YukkuriMovieMaker.Commons;

namespace NodeVideoEffects.Type
{
    public class VideoEffectsLoader : IDisposable
    {
        private readonly IVideoEffect? _videoEffect;
        private IVideoEffectProcessor? _processor;
        private readonly ShaderEffect? _shaderEffect;
        private readonly string _id;
        private readonly EffectType _type;

        private enum EffectType
        {
            VideoEffect,
            ShaderEffect
        }
        
        private VideoEffectsLoader(IVideoEffect? effect, string id)
        {
            _videoEffect = effect ?? throw new ArgumentNullException(nameof(effect), "Unable load effect");
            _type = EffectType.VideoEffect;
            _id = id;
        }

        private VideoEffectsLoader(ShaderEffect? effect, string id)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect), "Unable generate effect");
            _shaderEffect = effect;
            _type = EffectType.ShaderEffect;
            _id = id;
        }
        
        ~VideoEffectsLoader()
        {
            Dispose(false);
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
                        for (var i = 0; i < values.Length; i++)
                        {
                            _shaderEffect.SetValueByIndex(i, values[i]);
                        }
                    }
                    return this;
                }

                default:
                    return this;
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
                    if (image == null)
                    {
                        return false;
                    }
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _processor?.ClearInput();
            _processor?.Dispose();
            _processor = null;
        }

        public static async Task<VideoEffectsLoader> LoadEffect(string name, string id) =>
            await Task.Run(() => new VideoEffectsLoader(Activator.CreateInstance(PluginLoader.VideoEffects.ToList().First(type => type.Name == name)) as IVideoEffect, id));

        public static async Task<VideoEffectsLoader> LoadEffect(List<(System.Type type, string name)> properties, string shaderResourceId, string effectId)
        {
            return await Task.Run(() =>
            {
                if (shaderResourceId == "")
                    throw new ArgumentException("Shader resource id is empty.");
                var effect = ShaderEffect.Create(effectId, properties, shaderResourceId);
                if (effect.IsEnabled) return new VideoEffectsLoader(effect, effectId);
                effect.Dispose();
                effect = null;
                return new VideoEffectsLoader(effect, effectId);
            });
        }

        public abstract class ShaderEffect : D2D1CustomShaderEffectBase
        {
            public ShaderEffect(nint ptr) : base(ptr) { }

            public static ShaderEffect Create(string effectId, List<(System.Type type, string name)> properties, string shaderId)
            {
                // Generate a unique class name based on properties
                var className = $"ShaderEffect_{shaderId}_{string.Join("_", properties.Select(p => p.type.Name + p.name))}";
                var effectType = GenerateEffectType(className, properties, shaderId);

                var context = NodesManager.GetContext(effectId);
                var effectInstance = Activator.CreateInstance(effectType, context)
                                     ?? throw new InvalidOperationException("Cannot create effect instance");

                return effectInstance as ShaderEffect
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

            private static System.Type GenerateEffectType(string className, List<(System.Type, string)> properties, string shaderId)
            {
                AssemblyName assemblyName = new("DynamicID2D1PropertiesAssembly");
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                //PersistedAssemblyBuilder persistedAssemblyBuilder = new(assemblyName, typeof(object).Assembly);
                //ModuleBuilder moduleBuilder = persistedAssemblyBuilder.DefineDynamicModule("MainModule");
                var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

                var typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public, typeof(ShaderEffect));
                
                var effectImplType = DynamicEffectImplGenerator.GenerateEffectImpl(properties, shaderId, moduleBuilder);

                var index = 0;
                foreach (var( type, name) in properties)
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
                        System.Type.EmptyTypes
                    );

                    // Get the method info for the getter
                    var getterInfo = MakeGetterMethodInfo(type) ?? throw new InvalidOperationException("Cannot get the method \"GetValue\"");

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
                                   ?? throw new InvalidOperationException("Cannot get the method \"SetValue\""));
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
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    CallingConventions.Standard,
                    [typeof(IGraphicsDevicesAndContext)]);
                var ctorIl = ctorBuilder.GetILGenerator();
                ctorIl.Emit(OpCodes.Ldarg_0);
                ctorIl.Emit(OpCodes.Ldarg_1);
                ctorIl.Emit(OpCodes.Call, typeof(D2D1CustomShaderEffectBase).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                    .FirstOrDefault(m => m is { Name: "Create", IsGenericMethod: true })
                    ?.MakeGenericMethod(effectImplType)
                    ?? throw new InvalidOperationException("Cannot get Create method"));
                ctorIl.Emit(OpCodes.Call, typeof(ShaderEffect).GetConstructor([typeof(nint)])
                               ?? throw new InvalidOperationException("Cannot get the constructor"));
                ctorIl.Emit(OpCodes.Nop);
                ctorIl.Emit(OpCodes.Ret);

                var generateEffectType = typeBuilder.CreateTypeInfo().AsType();
                //persistedAssemblyBuilder.Save("EffectModule.dll");
                return generateEffectType
                       ?? throw new InvalidOperationException("Cannot create the type");

                MethodInfo? MakeGetterMethodInfo(System.Type type) => type switch
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
                    not null when type == typeof(Enum) => typeof(ShaderEffect).GetMethod("GetEnumValue")?.MakeGenericMethod(type),
                    not null when type == typeof(ComObject) => typeof(ShaderEffect).GetMethod("GetIUnknownValue")?.MakeGenericMethod(type),
                    not null when type == typeof(ID2D1ColorContext) => typeof(ShaderEffect).GetMethod("GetColorContextValue"),
                    _ => throw new InvalidOperationException("Unsupported type")
                };
            }
        }

        private static readonly Dictionary<string, byte[]> ShaderDictionaries = [];

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
    }
}
