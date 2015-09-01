using Data.States.Templates;

namespace Data.Interfaces
{
    public interface IRemoteServiceProviderDO : IBaseDO
    {
        int Id { get; set; }
        string Name { get; set; }
        _ServiceAuthorizationTypeTemplate AuthTypeTemplate { get; set; }
        string AppCreds { get; set; }
        string EndPoint { get; set; }
    }
}