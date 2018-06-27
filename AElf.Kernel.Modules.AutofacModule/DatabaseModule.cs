﻿﻿using AElf.Database;
using AElf.Database.Config;
using Autofac;

namespace AElf.Kernel.Modules.AutofacModule
{
    public class DatabaseModule : Module
    {
        private readonly IDatabaseConfig _config;

        public DatabaseModule(IDatabaseConfig config)
        {
            _config = config;
        }

        protected override void Load(ContainerBuilder builder)
        {
            switch (_config.Type)
            {
                case DatabaseType.Ssdb:
                    builder.RegisterType<SsdbDatabase>().WithParameter("config", _config).As<IKeyValueDatabase>()
                        .SingleInstance();
                    break;
                case DatabaseType.Redis:
                    builder.RegisterType<RedisDatabase>().WithParameter("config", _config).As<IKeyValueDatabase>()
                        .SingleInstance();
                    break;
                case DatabaseType.KeyValue:
                    builder.RegisterType<KeyValueDatabase>().As<IKeyValueDatabase>().SingleInstance();
                    break;
            }
        }
    }
}