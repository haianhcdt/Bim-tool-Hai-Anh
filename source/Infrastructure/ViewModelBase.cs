using System.Collections.Generic;using System.ComponentModel;using System.Runtime.CompilerServices;
namespace DSCons.Revit.Starter.Infrastructure;
public abstract class ViewModelBase:INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected bool SetProperty<T>(ref T storage,T value,[CallerMemberName]string? propertyName=null){if(EqualityComparer<T>.Default.Equals(storage,value))return false;storage=value;PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));return true;}
}
