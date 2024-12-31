using Microsoft.VisualBasic.FileIO;
using SharpGen.Runtime;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Mathematics;
using YukkuriMovieMaker.Player.Video;
using InvalidOperationException = System.InvalidOperationException;

namespace NodeVideoEffects.Type
{
    public static class DynamicEffectImplGenerator
    {
        public abstract class EffetImplBase : D2D1CustomShaderEffectImplBase<EffetImplBase>
        {
            public EffetImplBase(byte[] shaderBytes) : base(shaderBytes)
            {
            }

            protected override abstract void UpdateConstants();
        }

        private static readonly Dictionary<string, System.Type> TypeCache = new();

        unsafe public static System.Type GenerateEffectImpl(List<(System.Type type, string name)> fields, string shaderID, ModuleBuilder moduleBuild)
        {
            // Ensure unique type name based on shader name and field definitions
            string typeName = $"ShaderEffectImpl_{shaderID}_{string.Join("_", fields.Select(f => f.type.Name + f.name))}";

            if (TypeCache.TryGetValue(typeName, out System.Type? value))
            {
                return value;
            }

            // Define the EffectImpl class
            TypeBuilder effectImplTypeBuilder = moduleBuild.DefineType(
                typeName,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
                typeof(EffetImplBase)
            );

            ConstructorInfo customEffectAttributeConstructor = typeof(CustomEffectAttribute).GetConstructor(
                [typeof(int), typeof(string), typeof(string), typeof(string), typeof(string)])
                                                               ?? throw new InvalidOperationException("Cannot get the constructor");
            CustomAttributeBuilder customEffectAttributeBuilder = new(
            customEffectAttributeConstructor,
                [fields.Count, null, null, null, null]
            );
            effectImplTypeBuilder.SetCustomAttribute(customEffectAttributeBuilder);

            // Generate the ConstantBuffer struct
            TypeBuilder constantBufferTypeBuilder = moduleBuild.DefineType(
                $"ConstantBuffer_{shaderID}_{string.Join("_", fields.Select(f => f.type.Name + f.name))}",
                TypeAttributes.Public
                | TypeAttributes.SequentialLayout
                | TypeAttributes.AnsiClass
                | TypeAttributes.Sealed
                | TypeAttributes.BeforeFieldInit,
                typeof(ValueType));

            // Add fields to the struct
            foreach ((System.Type type, string name) field in fields)
            {
                constantBufferTypeBuilder.DefineField(field.name, field.type, FieldAttributes.Public);
            }
            System.Type constantBufferType = constantBufferTypeBuilder.CreateType();

            // Define the constantBuffer field
            FieldBuilder constantBufferField = effectImplTypeBuilder.DefineField(
                "constantBuffer",
                constantBufferTypeBuilder,
                FieldAttributes.Private
            );

            // Define properties based on fields
            for (int i = 0; i < fields.Count; i++)
            {
                (System.Type fieldType, string fieldName) = fields[i];

                //
                // Define getter
                //
                // get_{fieldName}() {
                //     return constantBufferField.{fieldName};
                // }
                //
                MethodBuilder getter = effectImplTypeBuilder.DefineMethod(
                    "get_" + fieldName,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    fieldType,
                    System.Type.EmptyTypes
                );
                ILGenerator getIL = getter.GetILGenerator();
                getIL.DeclareLocal(fieldType);
                Label getLabel = getIL.DefineLabel();
                getIL.Emit(OpCodes.Nop);
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldflda, constantBufferField);
                getIL.Emit(OpCodes.Ldfld, constantBufferType.GetField(fieldName)
                                          ?? throw new InvalidOperationException($"The field constantBuffer.{fieldName} not found."));
                getIL.Emit(OpCodes.Stloc_0);
                getIL.Emit(OpCodes.Br_S, getLabel);
                
                getIL.MarkLabel(getLabel);
                getIL.Emit(OpCodes.Ldloc_0);
                getIL.Emit(OpCodes.Ret);

                //
                // Define setter
                // set_{fieldName}(value) {
                //     constantBufferField.{fieldName} = value;
                //     UpdateConstants();
                // }
                //
                MethodBuilder setter = effectImplTypeBuilder.DefineMethod(
                    "set_" + fieldName,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    null,
                    [fieldType]
                );
                ILGenerator setIL = setter.GetILGenerator();

                // Set constantBuffer.{fieldName} = value
                setIL.Emit(OpCodes.Nop);
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldflda, constantBufferField);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, constantBufferType.GetField(fieldName)
                                          ?? throw new InvalidOperationException($"The field constantBuffer.{fieldName} not found."));

                // UpdateConstants()
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Callvirt, typeof(EffetImplBase).GetMethod(
                    "UpdateConstants",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                                             ?? throw new InvalidOperationException("Cannot get the method \"UpdateConstants\""));
                setIL.Emit(OpCodes.Nop);
                setIL.Emit(OpCodes.Ret);

                // Define property
                PropertyBuilder propertyBuilder = effectImplTypeBuilder.DefineProperty(
                    fieldName,
                    PropertyAttributes.None,
                    fieldType,
                    System.Type.EmptyTypes
                );

                // Map getter and setter to the property
                propertyBuilder.SetGetMethod(getter);
                propertyBuilder.SetSetMethod(setter);

                // Add CustomEffectProperty attribute
                // [CustomEffectProperty(PropertyType.{PropertyType}, i)]
                ConstructorInfo customEffectPropertyAttributeConstructor = typeof(CustomEffectPropertyAttribute).GetConstructor([typeof(PropertyType), typeof(int)]) ?? throw new InvalidOperationException("Cannot get the constructor");
                CustomAttributeBuilder customEffectPropertyAttributeBuilder = new(
                    customEffectPropertyAttributeConstructor,
                    [GetPropertyType(fieldType), i]
                );
                propertyBuilder.SetCustomAttribute(customEffectPropertyAttributeBuilder);
            }

            //
            // Define constructor
            // public EffectImpl() : base({shader}) { }
            //
            ConstructorBuilder constructor = effectImplTypeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                []
            );

            ILGenerator ctorIL = constructor.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Ldstr, shaderID);
            ctorIL.Emit(OpCodes.Call, typeof(VideoEffectsLoader).GetMethod("GetShader", BindingFlags.Public | BindingFlags.Static)
                                      ?? throw new InvalidOperationException("Cannot get the method \"GetShader\""));
            ctorIL.Emit(OpCodes.Call, typeof(EffetImplBase).GetConstructor([typeof(byte[])]) ?? throw new InvalidOperationException("Cannot get the constructor"));
            ctorIL.Emit(OpCodes.Nop);
            ctorIL.Emit(OpCodes.Nop);
            ctorIL.Emit(OpCodes.Ret);

            //
            // Define UpdateConstants method
            // unsafe protected override void UpdateConstants() {
            //     fixed(void* buffer = &constantBuffer) {
            //         typeof(ID2D1DrawInfo).GetMethod(
            //             "SetPixelShaderConstantBuffer", 
            //             BindingFlags.NonPublic | BindingFlags.Instance,
            //            [typeof(void*), typeof(int)])
            //         ?.Invoke(drawInfo, [(IntPtr)buffer, sizeof(ConstantBuffer)]);
            //     }
            // }
            //
            MethodBuilder updateConstantsMethod = effectImplTypeBuilder.DefineMethod(
                "UpdateConstants",
                MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                CallingConventions.Standard,
                null,
                []
            );

            ILGenerator updateIL = updateConstantsMethod.GetILGenerator();
            updateIL.DeclareLocal(typeof(void*));
            updateIL.DeclareLocal(constantBufferType.MakeByRefType(), true);
            var updateCallLabel = updateIL.DefineLabel();
            var updateRetLabel = updateIL.DefineLabel();
            
            // fixed(void* buffer = &constantBuffer) { ...
            updateIL.Emit(OpCodes.Ldarg_0);
            updateIL.Emit(OpCodes.Ldflda, constantBufferField);
            updateIL.Emit(OpCodes.Stloc_1);
            updateIL.Emit(OpCodes.Ldloc_1);
            updateIL.Emit(OpCodes.Conv_U);
            updateIL.Emit(OpCodes.Stloc_0);
            
            // MethodInfo? meth = typeof(ID2D1DrawInfo).GetMethod("SetPixelShaderConstantBuffer", BindingFlags.NonPublic | BindingFlags.Instance, [typeof(void).MakePointerType(), typeof(int)])
            updateIL.Emit(OpCodes.Ldtoken, typeof(ID2D1DrawInfo));
            updateIL.Emit(OpCodes.Call, typeof(System.Type).GetMethod("GetTypeFromHandle")
                                        ?? throw new InvalidOperationException("Cannot get the method \"GetTypeFromHandle\""));
            updateIL.Emit(OpCodes.Ldstr, "SetPixelShaderConstantBuffer");
            updateIL.Emit(OpCodes.Ldc_I4_S, 36);
            updateIL.Emit(OpCodes.Ldc_I4_2);
            updateIL.Emit(OpCodes.Newarr, typeof(System.Type));
            updateIL.Emit(OpCodes.Dup);
            updateIL.Emit(OpCodes.Ldc_I4_0);
            updateIL.Emit(OpCodes.Ldtoken, typeof(void*));
            updateIL.Emit(OpCodes.Call, typeof(System.Type).GetMethod("GetTypeFromHandle")
                                        ?? throw new InvalidOperationException("Cannot get the method \"GetTypeFromHandle\""));
            updateIL.Emit(OpCodes.Stelem_Ref);
            updateIL.Emit(OpCodes.Dup);
            updateIL.Emit(OpCodes.Ldc_I4_1);
            updateIL.Emit(OpCodes.Ldtoken, typeof(int));
            updateIL.Emit(OpCodes.Call, typeof(System.Type).GetMethod("GetTypeFromHandle")
                                        ?? throw new InvalidOperationException("Cannot get the method \"GetTypeFromHandle\""));
            updateIL.Emit(OpCodes.Stelem_Ref);
            updateIL.Emit(OpCodes.Call, typeof(System.Type).GetMethod("GetMethod", 
                                            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                                                [typeof(string), typeof(BindingFlags), typeof(System.Type[])])
                                            ?? throw new InvalidOperationException("Cannot get the method \"GetMethod\""));
            updateIL.Emit(OpCodes.Dup);
            updateIL.Emit(OpCodes.Brtrue_S, updateCallLabel);
            
            // if (meth is not null) {
            updateIL.Emit(OpCodes.Pop);
            updateIL.Emit(OpCodes.Br_S, updateRetLabel);
            
            //     meth.Invoke(drawInfo, [(IntPtr)buffer, sizeof(ConstantBuffer)]);
            // }
            updateIL.MarkLabel(updateCallLabel);
            updateIL.Emit(OpCodes.Ldarg_0);
            updateIL.Emit(OpCodes.Ldfld, typeof(EffetImplBase).GetField("drawInformation", BindingFlags.Instance | BindingFlags.NonPublic)
                                        ?? throw new InvalidOperationException("Cannot get the field \"drawInformation\""));
            updateIL.Emit(OpCodes.Ldc_I4_2);
            updateIL.Emit(OpCodes.Newarr, typeof(object));
            updateIL.Emit(OpCodes.Dup);
            updateIL.Emit(OpCodes.Ldc_I4_0);
            updateIL.Emit(OpCodes.Ldloc_0);
            updateIL.Emit(OpCodes.Box, typeof(IntPtr));
            updateIL.Emit(OpCodes.Stelem_Ref);
            updateIL.Emit(OpCodes.Dup);
            updateIL.Emit(OpCodes.Ldc_I4_1);
            updateIL.Emit(OpCodes.Sizeof, constantBufferType);
            updateIL.Emit(OpCodes.Box, typeof(int));
            updateIL.Emit(OpCodes.Stelem_Ref);
            updateIL.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("Invoke", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                                            [typeof(object), typeof(object[])])
                                        ?? throw new InvalidOperationException("Cannot get the method \"Invoke\""));
            updateIL.Emit(OpCodes.Pop);
            
            // return;
            updateIL.MarkLabel(updateRetLabel);
            updateIL.Emit(OpCodes.Ldc_I4_0);
            updateIL.Emit(OpCodes.Conv_U);
            updateIL.Emit(OpCodes.Stloc_1);
            updateIL.Emit(OpCodes.Ret);
            
            effectImplTypeBuilder.DefineMethodOverride(
                updateConstantsMethod,
                typeof(EffetImplBase).GetMethod("UpdateConstants", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    System.Type.EmptyTypes)
                    ?? throw new InvalidOperationException("Cannot get the method \"UpdateConstants\"")
                );

            // Create the type and cache it
            System.Type generatedType = effectImplTypeBuilder.CreateTypeInfo().AsType();
            TypeCache.Add(typeName, generatedType);
            return generatedType;
        }

        public static PropertyType GetPropertyType(System.Type type)
        {
            if (type == typeof(string))
                return PropertyType.String;
            if (type == typeof(bool))
                return PropertyType.Bool;
            if (type == typeof(uint))
                return PropertyType.UInt32;
            if (type == typeof(int))
                return PropertyType.Int32;
            if (type == typeof(float))
                return PropertyType.Float;
            if (type == typeof(Vector2))
                return PropertyType.Vector2;
            if (type == typeof(Vector3))
                return PropertyType.Vector3;
            if (type == typeof(Vector4))
                return PropertyType.Vector4;
            if (type == typeof(float[]))
                return PropertyType.Blob;
            if (type == typeof(IUnknown))
                return PropertyType.IUnknown;
            if (type == typeof(System.Enum))
                return PropertyType.Enum;
            if (type == typeof(Array))
                return PropertyType.Array;
            if (type == typeof(Guid))
                return PropertyType.Clsid;
            if (type == typeof(Matrix3x2))
                return PropertyType.Matrix3x2;
            if (type == typeof(Matrix4x3))
                return PropertyType.Matrix4x3;
            if (type == typeof(Matrix4x4))
                return PropertyType.Matrix4x4;
            if (type == typeof(Matrix5x4))
                return PropertyType.Matrix5x4;
            if (type == typeof(ID2D1ColorContext))
                return PropertyType.ColorContext;
            else
                return PropertyType.Unknown;
        }
    }
}
