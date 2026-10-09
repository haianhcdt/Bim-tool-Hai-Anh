using System;
using Autodesk.Revit.DB;

namespace DSCons.Revit.Starter.Infrastructure;

public static class TransactionRunner
{
    public static void Run(Document document, string name, Action action)
    {
        using var transaction = new Transaction(document, name);
        transaction.Start();
        try
        {
            action();
            transaction.Commit();
        }
        catch
        {
            if (transaction.GetStatus() == TransactionStatus.Started) transaction.RollBack();
            throw;
        }
    }
    public static T Run<T>(Document document,string name,Func<T> action){using var transaction=new Transaction(document,name);transaction.Start();try{var result=action();if(transaction.Commit()!=TransactionStatus.Committed)throw new InvalidOperationException($"Transaction '{name}' was not committed.");return result;}catch{if(transaction.GetStatus()==TransactionStatus.Started)transaction.RollBack();throw;}}
    public static T Preview<T>(Document document,string name,Func<T> action){using var group=new TransactionGroup(document,$"Preview - {name}");group.Start();try{var result=Run(document,name,action);group.RollBack();return result;}catch{if(group.GetStatus()==TransactionStatus.Started)group.RollBack();throw;}}
}
