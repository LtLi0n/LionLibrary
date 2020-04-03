using RestSharp;

namespace LionLibrary
{
    public class EntityResult<EntityT>
        where EntityT : class
    {
        public IRestResponse? Response { get; }
        public EntityT? Entity { get; }

        public EntityResult(IRestResponse? response, EntityT? entity)
        {
            Response = response;
            Entity = entity;
        }
    }
}
