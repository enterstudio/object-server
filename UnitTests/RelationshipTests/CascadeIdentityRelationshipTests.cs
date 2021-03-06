using System;
using NUnit.Framework;
using Nichevo.ObjectServer;
using System.EnterpriseServices;
using UnitTests.TestObjects;

namespace UnitTests.RelationshipTests
{
	[TestFixture]
	[Transaction(TransactionOption.RequiresNew)]
	public class CascadeIdentityRelationshipTests : ServicedComponent
	{
		private ObjectManager manager;

		[SetUp]
		public void Setup()
		{
			manager = new ObjectManager(ServerType.SqlServer, Constants.ConnectionString);
		}

		[TearDown]
		public void TearDown()
		{
			if(ContextUtil.IsInTransaction)
				ContextUtil.SetAbort();
		}

		[Test]
		public void SelectParentWithoutChildren()
		{
			int count = DataUtil.CountRows("IdentityParents");

			ObjectTransaction transaction = manager.BeginTransaction();
			
			CascadeIdentityParentTestObject parent = transaction.Select(typeof(CascadeIdentityParentTestObject), 4) as CascadeIdentityParentTestObject;
			Assert.AreEqual(4, parent.Id);	
			Assert.AreEqual("B", parent.ObjData);
			Assert.AreEqual(0, parent.ChildObjects.Count);	
		
			Assert.AreEqual(count, DataUtil.CountRows("IdentityParents"));
		}

		[Test]
		public void SelectParentWithChildren()
		{
			int count = DataUtil.CountRows("IdentityParents");

			ObjectTransaction transaction = manager.BeginTransaction();
			
			CascadeIdentityParentTestObject parent = transaction.Select(typeof(CascadeIdentityParentTestObject), 3) as CascadeIdentityParentTestObject;
			Assert.AreEqual(3, parent.Id);	
			Assert.AreEqual("A", parent.ObjData);
			Assert.AreEqual(2, parent.ChildObjects.Count);
	
			CascadeIdentityChildTestObject obj1 = parent.ChildObjects[0] as CascadeIdentityChildTestObject;
			Assert.AreEqual(1, obj1.Id);
			Assert.AreEqual("A", obj1.ObjData);
			Assert.AreEqual(3, obj1.Parent.Id);
			Assert.AreEqual("A", obj1.Parent.ObjData);
			Assert.AreEqual(2, obj1.Parent.ChildObjects.Count);

			CascadeIdentityChildTestObject obj2 = parent.ChildObjects[1] as CascadeIdentityChildTestObject;
			Assert.AreEqual(2, obj2.Id);
			Assert.AreEqual("B", obj2.ObjData);
			Assert.AreEqual(3, obj2.Parent.Id);
			Assert.AreEqual("A", obj2.Parent.ObjData);
			Assert.AreEqual(2, obj2.Parent.ChildObjects.Count);
	
			Assert.AreEqual(count, DataUtil.CountRows("IdentityParents"));
		}

		[Test]
		public void SelectChild()
		{
			int count = DataUtil.CountRows("IdentityChildren");

			ObjectTransaction transaction = manager.BeginTransaction();

			CascadeIdentityChildTestObject child = transaction.Select(typeof(CascadeIdentityChildTestObject), 1) as CascadeIdentityChildTestObject;
			Assert.AreEqual(1, child.Id);	
			Assert.AreEqual("A", child.ObjData);
			Assert.AreEqual(3, child.Parent.Id);	
			Assert.IsTrue(child.Parent.ChildObjects.Contains(child));

			Assert.AreEqual(count, DataUtil.CountRows("IdentityChildren"));
		}

		[Test]
		public void InsertParent()
		{
			int count = DataUtil.CountRows("IdentityParents");

			ObjectTransaction transaction = manager.BeginTransaction();
			
			CascadeIdentityParentTestObject parent = transaction.Create(typeof(CascadeIdentityParentTestObject)) as CascadeIdentityParentTestObject;
			parent.ObjData = "test";
			Assert.AreEqual(0, parent.ChildObjects.Count);

			transaction.Commit();

			Assert.AreEqual(count + 1, DataUtil.CountRows("IdentityParents"));
		}

		[Test]
		[ExpectedException(typeof(ObjectServerException), "UnitTests.TestObjects.CascadeIdentityChildTestObject.Parent has no value")]
		public void InsertChildWithNoParent()
		{
			ObjectTransaction transaction = manager.BeginTransaction();
			
			CascadeIdentityChildTestObject child = transaction.Create(typeof(CascadeIdentityChildTestObject)) as CascadeIdentityChildTestObject;
			child.ObjData = "test";

			transaction.Commit();
		}

		[Test]
		public void AddChild()
		{
			int parentCount = DataUtil.CountRows("IdentityParents");
			int childrenCount = DataUtil.CountRows("IdentityChildren");

			ObjectTransaction transaction = manager.BeginTransaction();
			
			CascadeIdentityParentTestObject parent = transaction.Select(typeof(CascadeIdentityParentTestObject), 4) as CascadeIdentityParentTestObject;
			Assert.AreEqual(4, parent.Id);	
			Assert.AreEqual("B", parent.ObjData);
			Assert.AreEqual(0, parent.ChildObjects.Count);
	
			CascadeIdentityChildTestObject child = transaction.Create(typeof(CascadeIdentityChildTestObject)) as CascadeIdentityChildTestObject;
			child.ObjData = "test";
			child.Parent = parent;

			Assert.AreEqual(4, child.Parent.Id);
			Assert.AreEqual(1, parent.ChildObjects.Count);

			Assert.IsTrue(parent.ChildObjects.Contains(child));

			transaction.Commit();

			Assert.AreEqual(parentCount, DataUtil.CountRows("IdentityParents"));
			Assert.AreEqual(childrenCount + 1, DataUtil.CountRows("IdentityChildren"));
		}

		[Test]
		public void UpdateChild()
		{
			int parentCount = DataUtil.CountRows("IdentityParents");
			int childrenCount = DataUtil.CountRows("IdentityChildren");

			ObjectTransaction transaction1 = manager.BeginTransaction();

			CascadeIdentityChildTestObject child1 = transaction1.Select(typeof(CascadeIdentityChildTestObject), 1) as CascadeIdentityChildTestObject;
			Assert.AreEqual(1, child1.Id);	
			Assert.AreEqual("A", child1.ObjData);
			Assert.AreEqual(3, child1.Parent.Id);	
			Assert.IsTrue(child1.Parent.ChildObjects.Contains(child1));

			child1.ObjData = "X";

			transaction1.Commit();

			Assert.AreEqual(parentCount, DataUtil.CountRows("IdentityParents"));
			Assert.AreEqual(childrenCount, DataUtil.CountRows("IdentityChildren"));

			ObjectTransaction transaction2 = manager.BeginTransaction();

			CascadeIdentityChildTestObject child2 = transaction2.Select(typeof(CascadeIdentityChildTestObject), 1) as CascadeIdentityChildTestObject;

			Assert.AreEqual(1, child2.Id);	
			Assert.AreEqual("X", child2.ObjData);
			Assert.AreEqual(3, child2.Parent.Id);	
			Assert.IsTrue(child2.Parent.ChildObjects.Contains(child2));

			Assert.AreEqual(parentCount, DataUtil.CountRows("IdentityParents"));
			Assert.AreEqual(childrenCount, DataUtil.CountRows("IdentityChildren"));
		}

		[Test]
		public void ShiftParents()
		{
			int parentCount = DataUtil.CountRows("IdentityParents");
			int childrenCount = DataUtil.CountRows("IdentityChildren");

			ObjectTransaction transaction1 = manager.BeginTransaction();

			CascadeIdentityParentTestObject parent1 = transaction1.Select(typeof(CascadeIdentityParentTestObject), 3) as CascadeIdentityParentTestObject;
			CascadeIdentityParentTestObject parent2 = transaction1.Select(typeof(CascadeIdentityParentTestObject), 4) as CascadeIdentityParentTestObject;

			CascadeIdentityChildTestObject child1 = transaction1.Select(typeof(CascadeIdentityChildTestObject), 1) as CascadeIdentityChildTestObject;

			Assert.AreEqual(1, child1.Id);	
			Assert.AreEqual("A", child1.ObjData);
			Assert.AreEqual(3, child1.Parent.Id);	
			Assert.IsTrue(child1.Parent.ChildObjects.Contains(child1));
			Assert.AreEqual(parent1, child1.Parent);
			Assert.IsTrue(parent1.ChildObjects.Contains(child1));
			Assert.IsFalse(parent2.ChildObjects.Contains(child1));

			child1.Parent = parent2;
			
			Assert.AreEqual(1, child1.Id);	
			Assert.AreEqual("A", child1.ObjData);
			Assert.AreEqual(4, child1.Parent.Id);	
			Assert.IsTrue(child1.Parent.ChildObjects.Contains(child1));
			Assert.AreEqual(parent2, child1.Parent);
			Assert.IsTrue(parent2.ChildObjects.Contains(child1));
			Assert.IsFalse(parent1.ChildObjects.Contains(child1));

			transaction1.Commit();

			Assert.AreEqual(parentCount, DataUtil.CountRows("IdentityParents"));
			Assert.AreEqual(childrenCount, DataUtil.CountRows("IdentityChildren"));

			ObjectTransaction transaction2 = manager.BeginTransaction();

			CascadeIdentityChildTestObject child2 = transaction2.Select(typeof(CascadeIdentityChildTestObject), 1) as CascadeIdentityChildTestObject;

			Assert.AreEqual(1, child2.Id);	
			Assert.AreEqual("A", child2.ObjData);
			Assert.AreEqual(4, child2.Parent.Id);	

			Assert.AreEqual(parentCount, DataUtil.CountRows("IdentityParents"));
			Assert.AreEqual(childrenCount, DataUtil.CountRows("IdentityChildren"));
		}

		[Test]
		public void InsertParentChild()
		{
			int parentCount = DataUtil.CountRows("IdentityParents");
			int childrenCount = DataUtil.CountRows("IdentityChildren");

			ObjectTransaction transaction = manager.BeginTransaction();

			CascadeIdentityParentTestObject parent = transaction.Create(typeof(CascadeIdentityParentTestObject)) as CascadeIdentityParentTestObject;
			parent.ObjData = "XXX";
			Assert.AreEqual(0, parent.ChildObjects.Count);

			for(int i = 0; i < 10; i++)
			{
				CascadeIdentityChildTestObject child = transaction.Create(typeof(CascadeIdentityChildTestObject)) as CascadeIdentityChildTestObject;
				child.ObjData = i.ToString();
				child.Parent = parent;
			}

			Assert.AreEqual(10, parent.ChildObjects.Count);

			transaction.Commit();

			Assert.AreEqual(parentCount + 1, DataUtil.CountRows("IdentityParents"));
			Assert.AreEqual(childrenCount + 10, DataUtil.CountRows("IdentityChildren"));
		}

		[Test]
		public void DeleteParentWithCascade()
		{
			int parentCount = DataUtil.CountRows("IdentityParents");
			int childrenCount = DataUtil.CountRows("IdentityChildren");

			ObjectTransaction transaction = manager.BeginTransaction();

			CascadeIdentityParentTestObject parent = transaction.Select(typeof(CascadeIdentityParentTestObject), 3) as CascadeIdentityParentTestObject;

			transaction.Delete(parent);

			transaction.Commit();

			Assert.AreEqual(parentCount - 1, DataUtil.CountRows("IdentityParents"));
			Assert.AreEqual(childrenCount - 2, DataUtil.CountRows("IdentityChildren"));
		}

		[Test]
		public void DeleteParent()
		{
			int parentCount = DataUtil.CountRows("IdentityParents");
			int childrenCount = DataUtil.CountRows("IdentityChildren");

			ObjectTransaction transaction = manager.BeginTransaction();

			CascadeIdentityParentTestObject parent = transaction.Select(typeof(CascadeIdentityParentTestObject), 3) as CascadeIdentityParentTestObject;

			foreach(CascadeIdentityChildTestObject child in parent.ChildObjects)
			{
				transaction.Delete(child);
			}

			transaction.Delete(parent);

			transaction.Commit();

			Assert.AreEqual(parentCount - 1, DataUtil.CountRows("IdentityParents"));
			Assert.AreEqual(childrenCount - 2, DataUtil.CountRows("IdentityChildren"));
		}
	}
}
