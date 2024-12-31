using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Effects;
using SharpGen.Runtime;
using System.Numerics;
using Vortice.Mathematics;
using System;
using System.Windows.Media.Effects;
using NodeVideoEffects.Utility;
using YukkuriMovieMaker.Commons;

namespace NodeVideoEffects.Type
{
    public class VideoEffectsLoader : IDisposable
    {
        IVideoEffect? videoEffect;
        IVideoEffectProcessor? processor;
        ShaderEffect? shaderEffect;
        string id;

        private VideoEffectsLoader(IVideoEffect? effect, string id)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect), "Unable load effect");
            videoEffect = effect;
            this.id = id;
        }

        private VideoEffectsLoader(ShaderEffect? effect, string id)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect), "Unable generate effect");
            shaderEffect = effect;
            this.id = id;
        }

        public void SetValue(string name, object? value)
        {
            if (shaderEffect != null)
            {
                lock (shaderEffect)
                {
                    shaderEffect.SetValue(name, value);
                }
            }
        }
        
        public void SetValue(int index, object? value)
        {
            if (shaderEffect != null)
            {
                lock (shaderEffect)
                {
                    shaderEffect.SetValue(index, value);
                }
            }
        }
        
        public void SetValue(params object[]? values)
        {
            if (shaderEffect != null && values != null)
            {
                lock (shaderEffect)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        shaderEffect.SetValue(i, values[i]);
                    }
                }
            }
        }

        public bool Update(ID2D1Image? image, out ID2D1Image? output)
        {
            output = null;
            if (videoEffect != null)
            {
                processor ??= videoEffect.CreateVideoEffect(NodesManager.GetContext(id));
                if (image == null)
                {
                    return false;
                }
                lock (processor)
                {
                    try
                    {
                        if (processor.Output.NativePointer == IntPtr.Zero)
                            processor = videoEffect.CreateVideoEffect(NodesManager.GetContext(id));
                        processor.SetInput(image);
                        processor.Update(NodesManager.GetInfo(id));
                        output = processor.Output;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            else if (shaderEffect != null)
            {
                if (shaderEffect == null) return false;
                lock (shaderEffect)
                {
                    try
                    {
                        shaderEffect.SetInput(0, image, true);
                        output = shaderEffect.Output;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            else return false;
        }

        public void Dispose()
        {
            processor?.ClearInput();
            processor?.Dispose();
            processor = null;
        }

        public static VideoEffectsLoader LoadEffect(string name, string id) =>
            new(Activator.CreateInstance(PluginLoader.VideoEffects.ToList().Where(type => type.Name == name).First()) as IVideoEffect, id);

        public static VideoEffectsLoader LoadEffect(List<(System.Type type, string name)> properties, string shaderResourceId, string effectId)
        {
            if (shaderResourceId == "")
                throw new ArgumentException("Shader resource id is empty.");
            ShaderEffect? effect = ShaderEffect.Create(effectId, properties, shaderResourceId);
            if (!effect.IsEnabled)
            {
                effect.Dispose();
                effect = null;
            }
            return new(effect, effectId);
        }

        public abstract class ShaderEffect : D2D1CustomShaderEffectBase
        {
            public static ShaderEffect Create(string effectId, List<(System.Type type, string name)> properties, string shaderId)
            {
                // Generate a unique class name based on properties
                string className = $"ShaderEffect_{shaderId}_{string.Join("_", properties.Select(p => p.type.Name + p.name))}";
                System.Type effectType = GenerateEffectType(className, properties, shaderId);

                object context = NodesManager.GetContext(effectId);
                object effectInstance = Activator.CreateInstance(effectType, [context])
                    ?? throw new InvalidOperationException("Cannot create effect instance");

                return effectInstance as ShaderEffect
                       ?? throw new InvalidOperationException("Cannot cast to ShaderEffect");
            }

            public void SetValue(string name, object? value)
            {
                PropertyInfo? property = GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

                if (property != null && property.CanWrite)
                {
                    property.SetValue(this, value);
                }
                else
                {
                    throw new ArgumentException($"Property '{name}' not found or is not writable.");
                }
            }

            public void SetValue(int index, object? value)
            {
                GetType().GetRuntimeProperties().ToArray()[index].SetValue(this, value);
            }

            public ShaderEffect(nint ptr):base(ptr)
            {
            }
            private static System.Type GenerateEffectType(string className, List<(System.Type, string)> properties, string shaderID)
            {
                AssemblyName assemblyName = new("DynamicID2D1PropertiesAssembly");
                AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                //PersistedAssemblyBuilder persistedAssemblyBuilder = new(assemblyName, typeof(object).Assembly);
                //ModuleBuilder moduleBuilder = persistedAssemblyBuilder.DefineDynamicModule("MainModule");
                ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

                TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public, typeof(ShaderEffect));

                System.Type effectImplType = DynamicEffectImplGenerator.GenerateEffectImpl(properties, shaderID, moduleBuilder);

                int index = 0;
                foreach ((System.Type type, string name) in properties)
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
                    MethodBuilder getterBuilder = typeBuilder.DefineMethod(
                        $"get_{name}",
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        type,
                        System.Type.EmptyTypes
                    );

                    // Get the method info for the getter
                    MethodInfo getterInfo = MakeGetterMethodInfo(type) ?? throw new InvalidOperationException("Cannot get the method \"GetValue\"");

                    ILGenerator getterIL = getterBuilder.GetILGenerator();
                    // return GetValue<{type}>({index});
                    getterIL.Emit(OpCodes.Ldarg_0);
                    getterIL.Emit(OpCodes.Ldc_I4, index);
                    getterIL.Emit(OpCodes.Call, getterInfo);
                    getterIL.Emit(OpCodes.Ret);

                    //
                    // Define setter
                    // set_{name}({type} value)
                    // {
                    //    SetValue({index}, value);
                    //    return;
                    // }
                    //
                    MethodBuilder setterBuilder = typeBuilder.DefineMethod(
                        $"set_{name}",
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                        null,
                        [type]
                    );

                    ILGenerator setterIL = setterBuilder.GetILGenerator();
                    // SetValue(index, value);
                    setterIL.Emit(OpCodes.Ldarg_0);
                    setterIL.Emit(OpCodes.Ldc_I4, index);
                    setterIL.Emit(OpCodes.Ldarg_1);
                    setterIL.Emit(OpCodes.Call, typeof(ID2D1Properties).GetMethod("SetValue", [typeof(int), type])
                                   ?? throw new InvalidOperationException("Cannot get the method \"SetValue\""));
                    getterIL.Emit(OpCodes.Nop);
                    setterIL.Emit(OpCodes.Ret);

                    // Define the property
                    // public {type} {name} { get => get_{name}(); set => set_{name}(value); }
                    PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
                        name,
                        PropertyAttributes.None,
                        type,
                        null
                    );

                    propertyBuilder.SetGetMethod(getterBuilder);
                    propertyBuilder.SetSetMethod(setterBuilder);
                }

                ConstructorBuilder ctorBuilder = typeBuilder.DefineConstructor(
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    CallingConventions.Standard,
                    [typeof(IGraphicsDevicesAndContext)]);
                ILGenerator ctorIL = ctorBuilder.GetILGenerator();
                ctorIL.Emit(OpCodes.Ldarg_0);
                ctorIL.Emit(OpCodes.Ldarg_1);
                ctorIL.Emit(OpCodes.Call, typeof(D2D1CustomShaderEffectBase).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                    .FirstOrDefault(m => m.Name == "Create" && m.IsGenericMethod)
                    ?.MakeGenericMethod(effectImplType)
                    ?? throw new InvalidOperationException("Cannot get Create method"));
                ctorIL.Emit(OpCodes.Call, typeof(ShaderEffect).GetConstructor([typeof(nint)])
                               ?? throw new InvalidOperationException("Cannot get the constructor"));
                ctorIL.Emit(OpCodes.Nop);
                ctorIL.Emit(OpCodes.Ret);

                var generateEffectType = typeBuilder.CreateTypeInfo().AsType();
                //persistedAssemblyBuilder.Save("EffectModule.dll");
                return generateEffectType
                       ?? throw new InvalidOperationException("Cannot create the type");

                MethodInfo? MakeGetterMethodInfo(System.Type type) => type switch
                {
                    System.Type t when t == typeof(bool) => typeof(ShaderEffect).GetMethod("GetBoolValue"),
                    System.Type t when t == typeof(Guid) => typeof(ShaderEffect).GetMethod("GetGuidValue"),
                    System.Type t when t == typeof(float) => typeof(ShaderEffect).GetMethod("GetFloatValue"),
                    System.Type t when t == typeof(int) => typeof(ShaderEffect).GetMethod("GetIntValue"),
                    System.Type t when t == typeof(Matrix3x2) => typeof(ShaderEffect).GetMethod("GetMatrix3x2Value"),
                    System.Type t when t == typeof(Matrix4x3) => typeof(ShaderEffect).GetMethod("GetMatrix4x3Value"),
                    System.Type t when t == typeof(Matrix4x4) => typeof(ShaderEffect).GetMethod("GetMatrix4x4Value"),
                    System.Type t when t == typeof(Matrix5x4) => typeof(ShaderEffect).GetMethod("GetMatrix5x4Value"),
                    System.Type t when t == typeof(string) => typeof(ShaderEffect).GetMethod("GetStringValue"),
                    System.Type t when t == typeof(uint) => typeof(ShaderEffect).GetMethod("GetUIntValue"),
                    System.Type t when t == typeof(Vector2) => typeof(ShaderEffect).GetMethod("GetVector2Value"),
                    System.Type t when t == typeof(Vector3) => typeof(ShaderEffect).GetMethod("GetVector3Value"),
                    System.Type t when t == typeof(Vector4) => typeof(ShaderEffect).GetMethod("GetVector4Value"),
                    System.Type t when t == typeof(Enum) => typeof(ShaderEffect).GetMethod("GetEnumValue")?.MakeGenericMethod(type),
                    System.Type t when t == typeof(ComObject) => typeof(ShaderEffect).GetMethod("GetIUnknownValue")?.MakeGenericMethod(type),
                    System.Type t when t == typeof(ID2D1ColorContext) => typeof(ShaderEffect).GetMethod("GetColorContextValue"),
                    _ => throw new InvalidOperationException("Unsupported type")
                };
            }
        }

        private static Dictionary<string, byte[]> shaderDictionaries = [];

        public static string RegisterShader(string shaderName)
        {
            byte[] shader;
            Assembly asm = Assembly.GetCallingAssembly();
            string resName = asm.GetManifestResourceNames().FirstOrDefault(a => a.EndsWith(shaderName), "");

            if (resName == "")
            {
                Logger.Write(LogLevel.Error, $"The shader resource \"*.{shaderName}\" not found.");
                return "";
            }

            using (Stream? resourceStream = asm.GetManifestResourceStream(resName))
            {
                if (resourceStream == null)
                {
                    Logger.Write(LogLevel.Error, $"The shader resource \"{resName}\" not found.");
                    return "";
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    resourceStream.CopyTo(memoryStream);
                    shader = memoryStream.ToArray();
                }
            }

            string id = Guid.NewGuid().ToString("N");
            shaderDictionaries.TryAdd(id, shader);
            return id;
        }

        public static byte[] GetShader(string id)
        {
            return shaderDictionaries[id];
        }
    }
}
