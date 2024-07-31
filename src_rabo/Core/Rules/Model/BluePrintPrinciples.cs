namespace Rabobank.Compliancy.Core.Rules.Model;

public static class BluePrintPrinciples
{
    public static readonly BluePrintPrinciple Auditability =
        new BluePrintPrinciple("All processes are auditable", true, false);
    public static readonly BluePrintPrinciple CodeIntegrity =
        new BluePrintPrinciple("Maintain (code) integrity including audit trail of code changes", true, true);
    public static readonly BluePrintPrinciple FourEyes =
        new BluePrintPrinciple("Enforce 4 eyes for every change to production", true, true);
    public static readonly BluePrintPrinciple SecurityTesting =
        new BluePrintPrinciple("Automate security testing where possible", true, false);
    public static readonly BluePrintPrinciple ReleaseMethod =
        new BluePrintPrinciple("Reduce impact by release methodology", false, false);
    public static readonly BluePrintPrinciple DataAccess =
        new BluePrintPrinciple("Prevent unauthorized data access", false, false);
}