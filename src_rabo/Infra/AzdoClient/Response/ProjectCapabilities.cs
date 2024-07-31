using System;
using System.Collections.Generic;
using System.Text;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class ProjectCapabilities
{
    public ProjectVersionControl Versioncontrol { get; set; }
    public ProjectProcessTemplate ProcessTemplate { get; set; }
}