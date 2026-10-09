using System;
using System.Collections.Generic;
namespace DSCons.Revit.Starter.Core.Models;
public sealed class OperationResult<T>
{
    private OperationResult(bool succeeded,T? value,IReadOnlyList<string> errors){Succeeded=succeeded;Value=value;Errors=errors;}
    public bool Succeeded{get;} public T? Value{get;} public IReadOnlyList<string> Errors{get;}
    public static OperationResult<T> Success(T value)=>new(true,value,Array.Empty<string>());
    public static OperationResult<T> Failure(params string[] errors)=>new(false,default,errors);
}
