using UnityEngine;

namespace _Project.Common
{
    public class CommandBase
    {
        private string _id;
        private string _description;

        public string Id => _id;
        public string Description => _description;

        public CommandBase(string id, string description)
        {
            _id = id;
            _description = description;
        }
    }
}