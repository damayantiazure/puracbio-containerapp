using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class SecurityNamespace
{
    public string Name { get; set; }

    public string NamespaceId { get; set; }
        
    public string DisplayName { get; set; }
        
    public string ReadPermission { get; set; }
        
    public string WritePermission { get; set; }
        
    public IEnumerable<NamespaceAction> Actions { get; set; }
}