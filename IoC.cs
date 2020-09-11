using System;
using System.Collections.Generic;

namespace ArchivalTibiaV71MapEditor
{
    public class IoC
    {
        private Dictionary<Type, object> _registrations = new Dictionary<Type, object>();
        private Dictionary<string, object> _namedRegistrations = new Dictionary<string, object>();
        public static IoC Instance { get; } = new IoC();

        public static void Register<TInterface, TConcrete>(TConcrete concrete) where TConcrete : TInterface
        {
            Instance._registrations.Add(typeof(TInterface), concrete);
        }
        
        public static void Register<TConcrete>(string name, TConcrete concrete)
        {
            Instance._namedRegistrations.Add(name, concrete);
        }
        
        public static TInterface Get<TInterface>()
        {
            return (TInterface)Instance._registrations[typeof(TInterface)];
        }
        
        public static T Get<T>(string name)
        {
            return (T)Instance._namedRegistrations[name];
        }
    }
}