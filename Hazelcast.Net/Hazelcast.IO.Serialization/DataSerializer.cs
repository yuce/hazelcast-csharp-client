using System;
using System.Collections.Generic;
using System.IO;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Serialization.Hook;
using Hazelcast.Transaction;
using Hazelcast.Util;

namespace Hazelcast.IO.Serialization
{
    internal sealed class DataSerializer : IStreamSerializer<IDataSerializable>
    {
        private const string FactoryId = "com.hazelcast.DataSerializerHook";

        private readonly IDictionary<int, IDataSerializableFactory> factories =
            new Dictionary<int, IDataSerializableFactory>();


        private readonly IDictionary<string, Type> class2Type = new Dictionary<string, Type>()
        {
            {"com.hazelcast.query.SqlPredicate", typeof(SqlPredicate)},
            {"com.hazelcast.transaction.TransactionOptions", typeof(TransactionOptions)}
        }; 
        internal DataSerializer(IDictionary<int, IDataSerializableFactory> dataSerializableFactories)
        {
            try
            {
                DataSerializerHook[] hooks =
                {
                    new ClusterDataSerializerHook(),
                    new SpiDataSerializerHook(),
                    new PartitionDataSerializerHook(),
                    new ClientDataSerializerHook(),
                    new MapDataSerializerHook(),
                    new QueueDataSerializerHook(),
                    new MultiMapDataSerializerHook(),
                    new CollectionDataSerializerHook(),
                    new ExecutorDataSerializerHook(),
                    new TopicDataSerializerHook(),
                    new LockDataSerializerHook(),
                    new SemaphoreDataSerializerHook(),
                    new AtomicLongDataSerializerHook(),
                    new CountDownLatchDataSerializerHook()
                };
                foreach (DataSerializerHook hook in hooks)
                {
                    IDataSerializableFactory factory = hook.CreateFactory();
                    if (factory != null)
                    {
                        Register(hook.GetFactoryId(), factory);
                    }
                }
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
            if (dataSerializableFactories != null)
            {
                foreach (var entry in dataSerializableFactories)
                {
                    Register(entry.Key, entry.Value);
                }
            }
        }

        public int GetTypeId()
        {
            return SerializationConstants.ConstantTypeData;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IDataSerializable Read(IObjectDataInput input)
        {
            IDataSerializable ds = null;
            bool identified = input.ReadBoolean();
            int id = 0;
            int factoryId = 0;
            string className = null;
            try
            {
                if (identified)
                {
                    factoryId = input.ReadInt();
                    IDataSerializableFactory dsf;
                    factories.TryGetValue(factoryId, out dsf);
                    if (dsf == null)
                    {
                        throw new HazelcastSerializationException(
                            "No DataSerializerFactory registered for namespace: " + factoryId);
                    }
                    id = input.ReadInt();
                    ds = dsf.Create(id);
                    if (ds == null)
                    {
                        throw new HazelcastSerializationException(dsf + " is not be able to create an instance for id: " +
                                                                  id);
                    }
                }
                else
                {
                    className = input.ReadUTF();
                    Type type = null;
                    class2Type.TryGetValue(className, out type);
                    if (type != null) ds = Activator.CreateInstance(type) as IDataSerializable;
                    if (ds == null)
                    {
                        throw new HazelcastSerializationException("Not able to create an instance for className: " +className);
                    }
                }
                ds.ReadData(input);
                return ds;
            }
            catch (Exception e)
            {
                if (e is IOException)
                {
                    throw;
                }
                if (e is HazelcastSerializationException)
                {
                    throw;
                }
                throw new HazelcastSerializationException(
                    "Problem while reading IDataSerializable, namespace: " + factoryId + ", id: " + id + ", class: " +
                    className + ", exception: " + e.Message, e);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(IObjectDataOutput output, IDataSerializable obj)
        {
            bool identified = obj is IIdentifiedDataSerializable;
            output.WriteBoolean(identified);
            if (identified)
            {
                var ds = (IIdentifiedDataSerializable) obj;
                output.WriteInt(ds.GetFactoryId());
                output.WriteInt(ds.GetId());
            }
            else
            {
                string javaClassName = obj.GetJavaClassName();
                if (!class2Type.ContainsKey(javaClassName))
                {
                    class2Type.Add(javaClassName, obj.GetType());
                }
                output.WriteUTF(javaClassName);
            }
            obj.WriteData(output);
        }

        public void Destroy()
        {
            factories.Clear();
        }

        private void Register(int factoryId, IDataSerializableFactory factory)
        {
            IDataSerializableFactory current;
            factories.TryGetValue(factoryId, out current);
            if (current != null)
            {
                if (current.Equals(factory))
                {
                    Logger.GetLogger(GetType())
                        .Warning("IDataSerializableFactory[" + factoryId + "] is already registered! Skipping " +
                                 factory);
                }
                else
                {
                    throw new ArgumentException("IDataSerializableFactory[" + factoryId + "] is already registered! " +
                                                current + " -> " + factory);
                }
            }
            else
            {
                factories.Add(factoryId, factory);
            }
        }
    }
}