using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Application;

public interface IChangeService
{
    Task CloseChangesAsync(CloseChangeDetails requestBody, IEnumerable<string> changeIds);
}