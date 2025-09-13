using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using SharpGen.Runtime;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Player.Video;
using InvalidOperationException = System.InvalidOperationException;

namespace NodeVideoEffects.Core;

public static class DynamicEffectImplGenerator
{
    private static readonly Dictionary<string, Type> TypeCache = new();

    public static Type GenerateEffectImpl(List<(Type type, string name)> fields, string shaderId,
        ModuleBuilder moduleBuild, int inputImageNum)
    {
        // Ensure unique type name based on shader name and field definitions
        var typeName = $"ShaderEffectImpl_{shaderId}_{string.Join("_", fields.Select(f => f.type.Name + f.name))}";

        if (TypeCache.TryGetValue(typeName, out var value)) return value;

        // Define the EffectImpl class
        var effectImplTypeBuilder = moduleBuild.DefineType(
            typeName,
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
            TypeAttributes.BeforeFieldInit,
            typeof(EffectImplBase)
        );

        var customEffectAttributeConstructor = typeof(CustomEffectAttribute).GetConstructor(
                                               [
                                                   typeof(int), typeof(string), typeof(string), typeof(string),
                                                   typeof(string)
                                               ])
                                               ?? throw new InvalidOperationException("Cannot get the constructor");
        var customEffectAttributeBuilder = new CustomAttributeBuilder(
            customEffectAttributeConstructor,
            [inputImageNum, null, null, null, null]
        );
        effectImplTypeBuilder.SetCustomAttribute(customEffectAttributeBuilder);

        // Generate the ConstantBuffer struct
        var constantBufferTypeBuilder = moduleBuild.DefineType(
            $"ConstantBuffer_{shaderId}_{string.Join("_", fields.Select(f => f.type.Name + f.name))}",
            TypeAttributes.Public
            | TypeAttributes.SequentialLayout
            | TypeAttributes.AnsiClass
            | TypeAttributes.Sealed
            | TypeAttributes.BeforeFieldInit,
            typeof(ValueType));

        // Add fields to the struct
        foreach (var field in fields)
            constantBufferTypeBuilder.DefineField(field.name, field.type, FieldAttributes.Public);
        for (var i = 0; i < 8; i++)
            constantBufferTypeBuilder.DefineField("margin" + i, typeof(int), FieldAttributes.Public);
        var constantBufferType = constantBufferTypeBuilder.CreateType();

        // Define the constantBuffer field
        var constantBufferField = effectImplTypeBuilder.DefineField(
            "constantBuffer",
            constantBufferTypeBuilder,
            FieldAttributes.Private
        );

        // Define properties based on fields
        for (var i = 0; i < fields.Count; i++)
        {
            var (fieldType, fieldName) = fields[i];

            //
            // Define getter
            //
            // get_{fieldName}() {
            //     return constantBufferField.{fieldName};
            // }
            //
            var getter = effectImplTypeBuilder.DefineMethod(
                "get_" + fieldName,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                fieldType,
                Type.EmptyTypes
            );
            var getIl = getter.GetILGenerator();
            getIl.DeclareLocal(fieldType);
            var getLabel = getIl.DefineLabel();
            getIl.Emit(OpCodes.Nop);
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldflda, constantBufferField);
            getIl.Emit(OpCodes.Ldfld, constantBufferType.GetField(fieldName)
                                      ?? throw new InvalidOperationException(
                                          $"The field constantBuffer.{fieldName} not found."));
            getIl.Emit(OpCodes.Stloc_0);
            getIl.Emit(OpCodes.Br_S, getLabel);

            getIl.MarkLabel(getLabel);
            getIl.Emit(OpCodes.Ldloc_0);
            getIl.Emit(OpCodes.Ret);

            //
            // Define setter
            // set_{fieldName}(value) {
            //     constantBufferField.{fieldName} = value;
            //     UpdateConstants();
            // }
            //
            var setter = effectImplTypeBuilder.DefineMethod(
                "set_" + fieldName,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                [fieldType]
            );
            var setIl = setter.GetILGenerator();

            // Set constantBuffer.{fieldName} = value
            setIl.Emit(OpCodes.Nop);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldflda, constantBufferField);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, constantBufferType.GetField(fieldName)
                                      ?? throw new InvalidOperationException(
                                          $"The field constantBuffer.{fieldName} not found."));

            // UpdateConstants()
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Callvirt, typeof(EffectImplBase).GetMethod(
                                             "UpdateConstants",
                                             BindingFlags.Instance | BindingFlags.NonPublic)
                                         ?? throw new InvalidOperationException(
                                             "Cannot get the method \"UpdateConstants\""));
            setIl.Emit(OpCodes.Nop);
            setIl.Emit(OpCodes.Ret);

            // Define property
            var propertyBuilder = effectImplTypeBuilder.DefineProperty(
                fieldName,
                PropertyAttributes.None,
                fieldType,
                Type.EmptyTypes
            );

            // Map getter and setter to the property
            propertyBuilder.SetGetMethod(getter);
            propertyBuilder.SetSetMethod(setter);

            // Add CustomEffectProperty attribute
            // [CustomEffectProperty(PropertyType.{PropertyType}, i)]
            var customEffectPropertyAttributeConstructor =
                typeof(CustomEffectPropertyAttribute).GetConstructor([typeof(PropertyType), typeof(int)]) ??
                throw new InvalidOperationException("Cannot get the constructor");
            CustomAttributeBuilder customEffectPropertyAttributeBuilder = new(
                customEffectPropertyAttributeConstructor,
                [GetPropertyType(fieldType), i]
            );
            propertyBuilder.SetCustomAttribute(customEffectPropertyAttributeBuilder);
        }

        List<MethodBuilder> marginGetter = new();
        for (var i = 0; i < 8; i++)
        {
            //
            // Define getter
            //
            // get_margin{0-7}() {
            //     return constantBufferField.margin{0-7};
            // }
            //
            var getter = effectImplTypeBuilder.DefineMethod(
                "get_margin" + i,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                typeof(int),
                Type.EmptyTypes
            );
            var getIl = getter.GetILGenerator();
            getIl.DeclareLocal(typeof(int));
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldflda, constantBufferField);
            getIl.Emit(OpCodes.Ldfld, constantBufferType.GetField("margin" + i)
                                      ?? throw new InvalidOperationException(
                                          $"The field constantBuffer.{"margin" + i} not found."));
            getIl.Emit(OpCodes.Ret);

            //
            // Define setter
            // set_margin{0-7}(value) {
            //     constantBufferField.margin{0-7} = value;
            // }
            //
            var setter = effectImplTypeBuilder.DefineMethod(
                "set_margin" + i,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                [typeof(int)]
            );
            var setIl = setter.GetILGenerator();

            // Set constantBuffer.margin{0-7} = value
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldflda, constantBufferField);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, constantBufferType.GetField("margin" + i)
                                      ?? throw new InvalidOperationException(
                                          $"The field constantBuffer.{"margin" + i} not found."));
            setIl.Emit(OpCodes.Ret);

            // Define property
            var propertyBuilder = effectImplTypeBuilder.DefineProperty(
                "margin" + i,
                PropertyAttributes.None,
                typeof(int),
                Type.EmptyTypes
            );

            // Map getter and setter to the property
            propertyBuilder.SetGetMethod(getter);
            propertyBuilder.SetSetMethod(setter);

            // Add CustomEffectProperty attribute
            // [CustomEffectProperty(PropertyType.{PropertyType}, i)]
            var customEffectPropertyAttributeConstructor =
                typeof(CustomEffectPropertyAttribute).GetConstructor([typeof(PropertyType), typeof(int)]) ??
                throw new InvalidOperationException("Cannot get the constructor");
            CustomAttributeBuilder customEffectPropertyAttributeBuilder = new(
                customEffectPropertyAttributeConstructor,
                [GetPropertyType(typeof(int)), i + fields.Count]
            );
            propertyBuilder.SetCustomAttribute(customEffectPropertyAttributeBuilder);
            marginGetter.Add(getter);
        }

        //
        // Define constructor
        // public EffectImpl() : base({shader}) { }
        //
        var constructor = effectImplTypeBuilder.DefineConstructor(
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
            MethodAttributes.RTSpecialName,
            CallingConventions.Standard,
            []
        );

        var ctorIl = constructor.GetILGenerator();
        ctorIl.Emit(OpCodes.Ldarg_0);
        ctorIl.Emit(OpCodes.Ldstr, shaderId);
        ctorIl.Emit(OpCodes.Call,
            typeof(VideoEffectsLoader).GetMethod("GetShader", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("Cannot get the method \"GetShader\""));
        ctorIl.Emit(OpCodes.Call,
            typeof(EffectImplBase).GetConstructor([typeof(byte[])]) ??
            throw new InvalidOperationException("Cannot get the constructor"));
        ctorIl.Emit(OpCodes.Nop);
        ctorIl.Emit(OpCodes.Nop);
        ctorIl.Emit(OpCodes.Ret);

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
        var updateConstantsMethod = effectImplTypeBuilder.DefineMethod(
            "UpdateConstants",
            MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig,
            CallingConventions.Standard,
            null,
            []
        );

        var updateIl = updateConstantsMethod.GetILGenerator();
        updateIl.DeclareLocal(typeof(void*));
        updateIl.DeclareLocal(constantBufferType.MakeByRefType(), true);
        var updateCallLabel = updateIl.DefineLabel();
        var updateRetLabel = updateIl.DefineLabel();

        // fixed(void* buffer = &constantBuffer) { ...
        updateIl.Emit(OpCodes.Ldarg_0);
        updateIl.Emit(OpCodes.Ldflda, constantBufferField);
        updateIl.Emit(OpCodes.Stloc_1);
        updateIl.Emit(OpCodes.Ldloc_1);
        updateIl.Emit(OpCodes.Conv_U);
        updateIl.Emit(OpCodes.Stloc_0);

        // MethodInfo? meth = typeof(ID2D1DrawInfo).GetMethod("SetPixelShaderConstantBuffer", BindingFlags.NonPublic | BindingFlags.Instance, [typeof(void).MakePointerType(), typeof(int)])
        updateIl.Emit(OpCodes.Ldtoken, typeof(ID2D1DrawInfo));
        updateIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle")
                                    ?? throw new InvalidOperationException(
                                        "Cannot get the method \"GetTypeFromHandle\""));
        updateIl.Emit(OpCodes.Ldstr, "SetPixelShaderConstantBuffer");
        updateIl.Emit(OpCodes.Ldc_I4_S, 36);
        updateIl.Emit(OpCodes.Ldc_I4_2);
        updateIl.Emit(OpCodes.Newarr, typeof(Type));
        updateIl.Emit(OpCodes.Dup);
        updateIl.Emit(OpCodes.Ldc_I4_0);
        updateIl.Emit(OpCodes.Ldtoken, typeof(void*));
        updateIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle")
                                    ?? throw new InvalidOperationException(
                                        "Cannot get the method \"GetTypeFromHandle\""));
        updateIl.Emit(OpCodes.Stelem_Ref);
        updateIl.Emit(OpCodes.Dup);
        updateIl.Emit(OpCodes.Ldc_I4_1);
        updateIl.Emit(OpCodes.Ldtoken, typeof(int));
        updateIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle")
                                    ?? throw new InvalidOperationException(
                                        "Cannot get the method \"GetTypeFromHandle\""));
        updateIl.Emit(OpCodes.Stelem_Ref);
        updateIl.Emit(OpCodes.Call, typeof(Type).GetMethod("GetMethod",
                                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                                        [typeof(string), typeof(BindingFlags), typeof(Type[])])
                                    ?? throw new InvalidOperationException("Cannot get the method \"GetMethod\""));
        updateIl.Emit(OpCodes.Dup);
        updateIl.Emit(OpCodes.Brtrue_S, updateCallLabel);

        // if (meth is not null) {
        updateIl.Emit(OpCodes.Pop);
        updateIl.Emit(OpCodes.Br_S, updateRetLabel);

        //     meth.Invoke(drawInfo, [(IntPtr)buffer, sizeof(ConstantBuffer)]);
        // }
        updateIl.MarkLabel(updateCallLabel);
        updateIl.Emit(OpCodes.Ldarg_0);
        updateIl.Emit(OpCodes.Ldfld,
            typeof(EffectImplBase).GetField("drawInformation", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Cannot get the field \"drawInformation\""));
        updateIl.Emit(OpCodes.Ldc_I4_2);
        updateIl.Emit(OpCodes.Newarr, typeof(object));
        updateIl.Emit(OpCodes.Dup);
        updateIl.Emit(OpCodes.Ldc_I4_0);
        updateIl.Emit(OpCodes.Ldloc_0);
        updateIl.Emit(OpCodes.Box, typeof(IntPtr));
        updateIl.Emit(OpCodes.Stelem_Ref);
        updateIl.Emit(OpCodes.Dup);
        updateIl.Emit(OpCodes.Ldc_I4_1);
        updateIl.Emit(OpCodes.Sizeof, constantBufferType);
        updateIl.Emit(OpCodes.Box, typeof(int));
        updateIl.Emit(OpCodes.Stelem_Ref);
        updateIl.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("Invoke",
                                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                                        [typeof(object), typeof(object[])])
                                    ?? throw new InvalidOperationException("Cannot get the method \"Invoke\""));
        updateIl.Emit(OpCodes.Pop);

        // return;
        updateIl.MarkLabel(updateRetLabel);
        updateIl.Emit(OpCodes.Ldc_I4_0);
        updateIl.Emit(OpCodes.Conv_U);
        updateIl.Emit(OpCodes.Stloc_1);
        updateIl.Emit(OpCodes.Ret);

        effectImplTypeBuilder.DefineMethodOverride(
            updateConstantsMethod,
            typeof(EffectImplBase).GetMethod("UpdateConstants",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                Type.EmptyTypes)
            ?? throw new InvalidOperationException("Cannot get the method \"UpdateConstants\"")
        );

        //
        // public override void MapInputRectsToOutputRect(RawRect[] inputRects,
        //     RawRect[] inputOpaqueSubRects,
        //     out RawRect outputRect,
        //     out RawRect outputOpaqueSubRect){
        //      outputRect = new RawRect(
        //          inputRects[0].Left - margin0,
        //          inputRects[0].Top - margin1, 
        //          inputRects[0].Right + margin2, 
        //          inputRects[0].Bottom + margin3);
        //      outputOpaqueSubRect = default;
        // }
        //
        var mapInputRectsToOutputRectMethod = effectImplTypeBuilder.DefineMethod(
            "MapInputRectsToOutputRect",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
            typeof(void),
            [
                typeof(RawRect[]), typeof(RawRect[]),
                typeof(RawRect).MakeByRefType(), typeof(RawRect).MakeByRefType()
            ]);

        var mapInputRectsToOutputRectIl = mapInputRectsToOutputRectMethod.GetILGenerator();

        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldarg_3);

        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldarg_1);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldc_I4_0);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldelema, typeof(RawRect));
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldfld, typeof(RawRect).GetField(nameof(RawRect.Left))
                                                        ?? throw new InvalidOperationException(
                                                            "Cannot get the field \"Left\""));
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldarg_0);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Call, marginGetter[0]);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Sub);

        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldarg_1);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldc_I4_0);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldelema, typeof(RawRect));
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldfld, typeof(RawRect).GetField(nameof(RawRect.Top))
                                                        ?? throw new InvalidOperationException(
                                                            "Cannot get the field \"Top\""));
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldarg_0);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Call, marginGetter[1]);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Sub);

        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldarg_1);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldc_I4_0);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldelema, typeof(RawRect));
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldfld, typeof(RawRect).GetField(nameof(RawRect.Right))
                                                        ?? throw new InvalidOperationException(
                                                            "Cannot get the field \"Right\""));
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldarg_0);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Call, marginGetter[2]);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Add);

        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldarg_1);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldc_I4_0);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldelema, typeof(RawRect));
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldfld, typeof(RawRect).GetField(nameof(RawRect.Bottom))
                                                        ?? throw new InvalidOperationException(
                                                            "Cannot get the field \"Bottom\""));
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldarg_0);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Call, marginGetter[3]);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Add);

        mapInputRectsToOutputRectIl.Emit(OpCodes.Newobj, typeof(RawRect).GetConstructors()[0]);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Stobj, typeof(RawRect));

        mapInputRectsToOutputRectIl.Emit(OpCodes.Ldarg_S, 4);
        mapInputRectsToOutputRectIl.Emit(OpCodes.Initobj, typeof(RawRect));
        mapInputRectsToOutputRectIl.Emit(OpCodes.Ret);

        effectImplTypeBuilder.DefineMethodOverride(
            mapInputRectsToOutputRectMethod,
            typeof(EffectImplBase).GetMethod("MapInputRectsToOutputRect")
            ?? throw new InvalidOperationException("Cannot get the method \"MapInputRectsToOutputRect\""));

        //
        // public override void MapOutputRectToInputRects(RawRect outputRect, RawRect[] inputRects)
        // {
        //     inputRects[0] = new RawRect(
        //         outputRect.Left - margin4,
        //         outputRect.Top - margin5,
        //         outputRect.Right + margin6,
        //         outputRect.Bottom + margin7);
        // }
        //
        var mapOutputRectToInputRectsMethod = effectImplTypeBuilder.DefineMethod(
            "MapOutputRectToInputRects",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
            typeof(void),
            [typeof(RawRect), typeof(RawRect[])]);

        var mapOutputRectToInputRectsIl = mapOutputRectToInputRectsMethod.GetILGenerator();

        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldarg_2);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldc_I4_0);

        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldarg_1);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldfld, typeof(RawRect).GetField(nameof(RawRect.Left))
                                                        ?? throw new InvalidOperationException(
                                                            "Cannot get the field \"Left\""));
        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldarg_0);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Call, marginGetter[4]);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Sub);

        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldarg_1);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldfld, typeof(RawRect).GetField(nameof(RawRect.Top))
                                                        ?? throw new InvalidOperationException(
                                                            "Cannot get the field \"Top\""));
        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldarg_0);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Call, marginGetter[5]);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Sub);

        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldarg_1);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldfld, typeof(RawRect).GetField(nameof(RawRect.Right))
                                                        ?? throw new InvalidOperationException(
                                                            "Cannot get the field \"Right\""));
        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldarg_0);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Call, marginGetter[6]);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Add);

        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldarg_1);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldfld, typeof(RawRect).GetField(nameof(RawRect.Bottom))
                                                        ?? throw new InvalidOperationException(
                                                            "Cannot get the field \"Bottom\""));
        mapOutputRectToInputRectsIl.Emit(OpCodes.Ldarg_0);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Call, marginGetter[7]);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Add);

        mapOutputRectToInputRectsIl.Emit(OpCodes.Newobj, typeof(RawRect).GetConstructors()[0]);
        mapOutputRectToInputRectsIl.Emit(OpCodes.Stelem, typeof(RawRect));
        mapOutputRectToInputRectsIl.Emit(OpCodes.Ret);

        effectImplTypeBuilder.DefineMethodOverride(
            mapOutputRectToInputRectsMethod,
            typeof(EffectImplBase).GetMethod("MapOutputRectToInputRects")
            ?? throw new InvalidOperationException("Cannot get the method \"MapOutputRectToInputRects\""));

        // Create the type and cache it
        var generatedType = effectImplTypeBuilder.CreateTypeInfo().AsType();
        TypeCache.Add(typeName, generatedType);
        return generatedType;
    }

    private static PropertyType GetPropertyType(Type type)
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
        return PropertyType.Unknown;
    }

    public abstract class EffectImplBase : D2D1CustomShaderEffectImplBase<EffectImplBase>
    {
        public EffectImplBase(byte[] shaderBytes) : base(shaderBytes)
        {
        }

        protected abstract override void UpdateConstants();
    }
}