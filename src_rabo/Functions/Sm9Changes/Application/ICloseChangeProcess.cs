using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Application;

public interface ICloseChangeProcess
{
    public Task<(IEnumerable<string>, IEnumerable<string>)> CloseChangeAsync(CloseChangeRequest closeChangeRequest);
}