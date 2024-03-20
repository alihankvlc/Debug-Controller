using System;
using UnityEngine;

namespace _Project.Common
{
    public class Command : CommandBase
    {
        private Action _process;

        public Command(string id, string description, Action process) : base(id, description)
        {
            _process = process;
        }

        public void Use()
        {
            _process?.Invoke();
            Debug.Log($"{Description}");
        }
    }

    public class Command<T> : CommandBase where T : struct
    {
        private Action<T> _processWithArgument;

        public Command(string id, string description, Action<T> process) : base(id, description)
        {
            _processWithArgument = process;
        }

        public void Use(T argument)
        {
            _processWithArgument?.Invoke(argument);
            Debug.Log($"{Description}");
        }
    }
}