namespace Philips.EDI.Foundation.APIGateway.AutomationTest.Models
{
    public enum TestCategory
    {
        GatedSanity,
        Nightly,
        PostDeployment,
        WithoutMultitenancy,
        OrgNameInUrl,
        OrgIdInHeader,
        UserAccessToken,
        ServiceIDAccessToken,
        BrowserLogout,
        SSOToken,        
        IAMTokenExchangeBroker,
        IAMTokenExchangeBrokerPreCondition,
        IDTokenValidator,
        OnPrem,
        UpgradeSanity,
        IntegratedSanity

    }

    public enum ExecutionEnvironment
    {        
        Local,
        Production
    }
}
