using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DeluxeCarsUI.Model;
using DeluxeCarsUI.Repositories;

namespace DeluxeCarsUI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private UserAccountModel _currentUserAccount;
        private IUserRepository userRepository;
        public UserAccountModel CurrentUserAccount
        {
            get
            {
                return _currentUserAccount;
            }
            set
            {
                _currentUserAccount = value;
                OnPropertyChanged(nameof(CurrentUserAccount));
            }
        }

        public MainViewModel()
        {
            userRepository = new UserRepository();
            LoadCurrentUserData();
        }

        private void LoadCurrentUserData()
        {
            var user = userRepository.GetByUsername(Thread.CurrentPrincipal.Identity.Name);
            if (user != null)
            {
                if (CurrentUserAccount == null)
                {
                    CurrentUserAccount = new UserAccountModel(); // Asegúrate de usar el tipo correcto
                }
                CurrentUserAccount.Username = user.Username;
                CurrentUserAccount.DisplayName = $"Welcome {user.LastName}";
                CurrentUserAccount.ProfilePicture = null;
            }
            else
            {
                if (CurrentUserAccount == null)
                {
                    CurrentUserAccount = new UserAccountModel();
                }
                CurrentUserAccount.DisplayName = "Invalid user";
            }
        }
    }
}
