using DeluxeCarsEntities;

namespace DeluxeCarsShared.Interfaces
{
    public interface ICurrentUserService
    {
        int? CurrentUserId { get; }
        bool IsAdmin { get; }
        void SetCurrentUser(Usuario user);
        void ClearCurrentUser();
    }
}
