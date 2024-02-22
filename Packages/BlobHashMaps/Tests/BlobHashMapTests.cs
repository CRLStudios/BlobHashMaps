using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CRL.BlobHashMaps.Tests
{
    public class BlobHashMapTests
    {
        [Test]
        public void HashMapsFull()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BlobHashMap<int2, int>>();
                var hashMapBuilder = builder.AllocateHashMap(ref root, 5);
                for (int i = 0; i < 10; i++)
                {
                    hashMapBuilder.Add(new int2(i, 0), 0);
                }
            });

            Assert.Throws<InvalidOperationException>(() =>
            {
                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BlobMultiHashMap<int2, int>>();
                var hashMapBuilder = builder.AllocateMultiHashMap(ref root, 5);
                for (int i = 0; i < 10; i++)
                {
                    hashMapBuilder.Add(new int2(i, 0), 0);
                }
            });
        }
        
        [Test]
        public void HashMapsZeroCapacity()
        {
            // Capacity 0 hashmap
            Assert.Throws<ArgumentException>(() =>
            {
                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BlobHashMap<int2, int>>();
                builder.AllocateHashMap(ref root, 0);
            });

            // Capacity 0 multihashmap
            Assert.Throws<ArgumentException>(() =>
            {
                BlobBuilder builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BlobMultiHashMap<int2, int>>();
                builder.AllocateMultiHashMap(ref root, 0);
            });
        }

        [Test]
        public void HashMapsEmpty()
        {
            // Zero elements with capacity > 0 hashmap read:
            Assert.DoesNotThrow(() =>
            {
                // zero elements with capacity
                BlobBuilder builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BlobHashMap<int2, int>>();
                var hashMapBuilder = builder.AllocateHashMap(ref root, 10);
                var blobRef = builder.CreateBlobAssetReference<BlobHashMap<int2, int>>(Allocator.Persistent);

                if (blobRef.Value.TryGetValue(int2.zero, out _))
                    throw new Exception("true was returned by TryGetValue while the hashmap is empty");

                blobRef.Dispose();
            });
            
            // Zero elements with capacity > 0 multihashmap read:
            Assert.DoesNotThrow(() =>
            {
                // zero elements with capacity
                BlobBuilder builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BlobMultiHashMap<int2, int>>();
                var hashMapBuilder = builder.AllocateMultiHashMap(ref root, 10);
                var blobRef = builder.CreateBlobAssetReference<BlobMultiHashMap<int2, int>>(Allocator.Persistent);
                if (blobRef.Value.TryGetFirstValue(int2.zero, out _, out _))
                    throw new Exception("true was returned by TryGetValue while the multihashmap is empty");

                blobRef.Dispose();
            });
        }

        [Test]
        public void HashMapAddContainsKey()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BlobHashMap<int, int>>();
                var hashMapBuilder = builder.AllocateHashMap(ref root, 10);
                hashMapBuilder.Add(420, 69);
                hashMapBuilder.Add(420, 69);
            },"An item with key 420 already exists");
        }

        [Test]
        public void HashMapExactlyFull()
        {
            Assert.DoesNotThrow(() =>
            {
                int size = 10;
                
                // zero elements with capacity
                BlobBuilder builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BlobHashMap<int, int>>();
                var hashMapBuilder = builder.AllocateHashMap(ref root, size);
                
                for (int i = 0; i < size; i++)
                    hashMapBuilder.Add(i, i);

                var blobRef = builder.CreateBlobAssetReference<BlobHashMap<int, int>>(Allocator.Persistent);

                for (int i = 0; i < size; i++)
                {
                    int value = blobRef.Value[i];
                    Assert.IsTrue(value == i);
                }

                blobRef.Dispose();
            });
        }
        
        [Test]
        public void MultiHashMapAddDuplicateKey()
        {
            Assert.DoesNotThrow(() =>
            {
                BlobBuilder builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<BlobMultiHashMap<FixedString32Bytes, int>>();
                var hashMapBuilder = builder.AllocateMultiHashMap(ref root, 10);
                
                hashMapBuilder.Add("420", 1);
                hashMapBuilder.Add("420", 2);
                hashMapBuilder.Add("420", 3);
                hashMapBuilder.Add("69", 1);
                hashMapBuilder.Add("69", 2);
                hashMapBuilder.Add("69", 3);
            });
        }

        [Test]
        public void HashMapKeyNotPresent()
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BlobHashMap<FixedString32Bytes, int>>();
            var hashMapBuilder = builder.AllocateHashMap(ref root, 10);
            hashMapBuilder.Add("420", 69);
            var blobRef = builder.CreateBlobAssetReference<BlobHashMap<int, int>>(Allocator.Persistent);
            int dummy = 0;
            Assert.Throws<KeyNotFoundException>(() => dummy = blobRef.Value[0] + blobRef.Value[1]);
        }

        [Test]
        public void HashMapSumValues()
        {
            int size = 42;
            
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BlobHashMap<int, int>>();
            var hashMapBuilder = builder.AllocateHashMap(ref root, size);

            int expectedSum = 0;
            for (int i = 0; i < size; i++)
            {
                hashMapBuilder.Add(i, i);
                expectedSum += i;
            }
                
            var blobRef = builder.CreateBlobAssetReference<BlobHashMap<int, int>>(Allocator.Persistent);

            int sum = 0;
            for (int i = 0; i < size; i++)
            {
                int value = blobRef.Value[i];
                sum += value;
            }

            Assert.IsTrue(sum == expectedSum);
        }
        
        [Test]
        public void MultiHashMapSumValues()
        {
            int size = 100;
            
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BlobMultiHashMap<int, int>>();
            var hashMapBuilder = builder.AllocateMultiHashMap(ref root, size);
            
            int expectedSum = 0;
            for (int i = 0; i < size; i++)
            {
                // test multiple same keys too
                hashMapBuilder.Add(i / 5, i);
                expectedSum += i;
            }
                
            var blobRef = builder.CreateBlobAssetReference<BlobMultiHashMap<int, int>>(Allocator.Persistent);

            int sum = 0;
            for (int i = 0; i < size; i++)
            {
                if (blobRef.Value.TryGetFirstValue(i, out int value, out var it))
                {
                    do
                    {
                        sum += value;
                    } while (blobRef.Value.TryGetNextValue(out value, ref it));
                }
            }
          
            Assert.IsTrue(sum == expectedSum);
        }

        [Test]
        public void HashmapCheckKeys()
        {
            int size = 10;
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BlobHashMap<int, int>>();
            var hashMapBuilder = builder.AllocateHashMap(ref root, size * 2);

            int expectedSum = 0;
            for (int i = 0; i < size; i++)
            {
                hashMapBuilder.TryAdd(i, i);
                expectedSum += i;
            }
                
            var blobRef = builder.CreateBlobAssetReference<BlobHashMap<int, int>>(Allocator.Persistent);

            var keys = blobRef.Value.GetKeyArray(Allocator.Temp);
            var values = blobRef.Value.GetValueArray(Allocator.Temp);

            int sum = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                Assert.AreEqual(keys[i], values[i]);
                // UnityEngine.Assertions.Assert.AreEqual(keys[i], values[i]);
                sum += values[i];
            }
            
            Assert.AreEqual(expectedSum, sum);
        }
        
        [Test]
        public void MultiHashmapCheckKeys()
        {
            int size = 10;
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BlobMultiHashMap<int, int>>();
            var hashMapBuilder = builder.AllocateMultiHashMap(ref root, size * 2); // test capacity > items were adding

            int expectedSum = 0;
            for (int i = 0; i < size; i++)
            {
                hashMapBuilder.Add(0, i);
                
                expectedSum += i;
            }
                
            var blobRef = builder.CreateBlobAssetReference<BlobMultiHashMap<int, int>>(Allocator.Persistent);

            var keys = blobRef.Value.GetKeyArray(Allocator.Temp);
            var values = blobRef.Value.GetValueArray(Allocator.Temp);

            Assert.IsTrue(values.Length == size);
            int sum = 0;
            for (int i = 0; i < values.Length; i++)
            {
                sum += values[i];
            }
            
            Assert.AreEqual(expectedSum, sum);
        }
    }
}