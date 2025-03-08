using System.Diagnostics;
using System.Reflection;
using ExceptionDetailsInfo = Microsoft.ApplicationInsights.DataContracts.ExceptionDetailsInfo;

namespace CatalogueScanner.Core.Host.ApplicationInsights;

internal static class ExceptionConverter
{
    private static readonly Lazy<ExceptionDetailsMethods> exceptionDetailsMethods = new(() =>
    {
        var exceptionDetailsType = GetType("Microsoft.ApplicationInsights.Extensibility.Implementation.External.ExceptionDetails, Microsoft.ApplicationInsights, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        var createWithoutStackInfoMethod = GetMethod(exceptionDetailsType, "CreateWithoutStackInfo", BindingFlags.NonPublic | BindingFlags.Static);
        var parsedStackProperty = GetProperty(exceptionDetailsType, "parsedStack");
        var hasFullStackProperty = GetProperty(exceptionDetailsType, "hasFullStack");

        var stackFrameType = GetType("Microsoft.ApplicationInsights.Extensibility.Implementation.External.StackFrame, Microsoft.ApplicationInsights, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

        var exceptionConverterType = GetType("Microsoft.ApplicationInsights.Extensibility.Implementation.ExceptionConverter, Microsoft.ApplicationInsights, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        var getStackFrameMethod = GetMethod(exceptionConverterType, "GetStackFrame", BindingFlags.NonPublic | BindingFlags.Static);
        var getStackFrameLengthMethod = GetMethod(exceptionConverterType, "GetStackFrameLength", BindingFlags.NonPublic | BindingFlags.Static);
        var sanitizeStackFrameMethod = GetMethod(exceptionConverterType, "SanitizeStackFrame", BindingFlags.NonPublic | BindingFlags.Static);

        var sanitizeStackFrameGenericMethod = sanitizeStackFrameMethod.MakeGenericMethod(typeof(StackFrame), stackFrameType);
        var converterDelegateType = sanitizeStackFrameGenericMethod.GetParameters()[1].ParameterType;
        var lengthGetterDelegateType = sanitizeStackFrameGenericMethod.GetParameters()[2].ParameterType;

        var sanitizedTupleType = sanitizeStackFrameGenericMethod.ReturnType;
        var item1Property = GetProperty(sanitizedTupleType, "Item1");
        var item2Property = GetProperty(sanitizedTupleType, "Item2");

        var exceptionDetailsInfoConstructor = typeof(ExceptionDetailsInfo).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, [exceptionDetailsType])
            ?? throw new InvalidOperationException($"Constructor {typeof(ExceptionDetailsInfo).FullName}({exceptionDetailsType.FullName}) not found");

        return new(
            createWithoutStackInfoMethod,
            parsedStackProperty,
            hasFullStackProperty,
            sanitizeStackFrameGenericMethod,
            getStackFrameMethod,
            getStackFrameLengthMethod,
            converterDelegateType,
            lengthGetterDelegateType,
            item1Property,
            item2Property,
            exceptionDetailsInfoConstructor
        );
    });

    /// <summary>
    /// Creates an <see cref="ExceptionDetailsInfo"/> instance from an exception and custom stack trace. The exception's own stack trace will be ignored.
    /// </summary>
    /// <param name="exception">The exception</param>
    /// <param name="stackTrace">The custom stack trace to use</param>
    /// <returns>The ExceptionDetailsInfo instace</returns>
    public static ExceptionDetailsInfo ConvertToExceptionDetailsInfo(Exception exception, StackTrace stackTrace)
    {
        var methods = exceptionDetailsMethods.Value;

        var exceptionDetails = methods.CreateWithoutStackInfo.Invoke(null, [exception, null])
            ?? throw new InvalidOperationException($"{methods.CreateWithoutStackInfo.Name} returned null");

        var frames = stackTrace.GetFrames();

        var sanitizedTuple = methods.SanitizeStackFrame.Invoke(null, [
            frames,
            methods.GetStackFrame.CreateDelegate(methods.ConverterDelegateType),
            methods.GetStackFrameLength.CreateDelegate(methods.LengthGetterDelegateType)
        ])
            ?? throw new InvalidOperationException($"{methods.SanitizeStackFrame.Name} returned null");

        methods.ParsedStack.SetValue(exceptionDetails, methods.SanitizedTupleItem1.GetValue(sanitizedTuple));
        methods.HasFullStack.SetValue(exceptionDetails, methods.SanitizedTupleItem2.GetValue(sanitizedTuple));

        var exceptionDetailsInfo = (ExceptionDetailsInfo?)methods.ExceptionDetailsInfoConstructor.Invoke([exceptionDetails])
            ?? throw new InvalidOperationException($"{typeof(ExceptionDetailsInfo).FullName} instance is null");

        return exceptionDetailsInfo;
    }

    private static Type GetType(string typeName) =>
        Type.GetType(typeName)
            ?? throw new InvalidOperationException($"Type {typeName} not found");

    private static MethodInfo GetMethod(Type type, string methodName, BindingFlags bindingAttr) =>
        type.GetMethod(methodName, bindingAttr)
            ?? throw new InvalidOperationException($"Method {type.FullName}.{methodName} not found");

    private static PropertyInfo GetProperty(Type type, string propertyName) =>
        type.GetProperty(propertyName)
            ?? throw new InvalidOperationException($"Property {type.FullName}.{propertyName} not found");

    internal sealed record ExceptionDetailsMethods(
        MethodInfo CreateWithoutStackInfo,
        PropertyInfo ParsedStack,
        PropertyInfo HasFullStack,
        MethodInfo SanitizeStackFrame,
        MethodInfo GetStackFrame,
        MethodInfo GetStackFrameLength,
        Type ConverterDelegateType,
        Type LengthGetterDelegateType,
        PropertyInfo SanitizedTupleItem1,
        PropertyInfo SanitizedTupleItem2,
        ConstructorInfo ExceptionDetailsInfoConstructor
    )
    { }
}
