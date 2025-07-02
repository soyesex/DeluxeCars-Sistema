using DeluxeCars.DataAccess.Repositories.Interfaces;
using DeluxeCarsEntities;

namespace DeluxeCars.DataAccess.Repositories.Implementations
{
    public class NotaDeCreditoRepository : GenericRepository<NotaDeCredito>, INotaDeCreditoRepository
    {
        public NotaDeCreditoRepository(AppDbContext context) : base(context)
        {
        }
    }
}
