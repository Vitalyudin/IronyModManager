﻿// ***********************************************************************
// Assembly         : IronyModManager.DI
// Author           : Mario
// Created          : 06-10-2020
//
// Last Modified By : Mario
// Last Modified On : 06-10-2020
// ***********************************************************************
// <copyright file="MessageBusRegistration.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using IronyModManager.Shared;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Memory;

namespace IronyModManager.DI.MessageBus
{
    /// <summary>
    /// Class MessageBusRegistration.
    /// </summary>
    public static class MessageBusRegistration
    {
        #region Methods

        /// <summary>
        /// Registers this instance.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        public static void Register(IEnumerable<Assembly> assemblies)
        {
            var builder = MessageBusBuilder.Create()
                .WithDependencyResolver(new MessageBusDependencyResolver())
                .WithProviderMemory(new MemoryMessageBusSettings()
                {
                    EnableMessageSerialization = false
                })
                .Do(builder =>
                {
                    assemblies.ToList().ForEach(assembly =>
                    {
                        assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract)
                            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
                            .Where(x => x.Interface.IsGenericType && x.Interface.GetGenericTypeDefinition() == typeof(IIronyMessageBusConsumer<>))
                            .Select(x => new { HandlerType = x.Type, EventType = x.Interface.GetGenericArguments()[0] })
                            .ToList()
                            .ForEach(find =>
                            {
                                builder.Consume(find.EventType, x => x.Topic(x.MessageType.Name).WithConsumer(find.HandlerType));
                            });
                    });
                });
            var mbus = new IronyMessageBus(builder.Build());
            DIContainer.Container.RegisterInstance(mbus);
        }

        #endregion Methods
    }
}
