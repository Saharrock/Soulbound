using System.ComponentModel;

using System.Runtime.CompilerServices;



namespace Soulbound.ViewModels

{

    // Базовый класс ViewModel: уведомляет UI об изменении свойств (INotifyPropertyChanged).

    internal class ViewModelBase : INotifyPropertyChanged

    {

        public event PropertyChangedEventHandler? PropertyChanged;



        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)

        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

    }

}


