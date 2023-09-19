using NUnit.Framework;
using System.Linq;

namespace Fury.Tests
{
    public class EntriesTests
    {
        private sealed class User
        {
            public readonly int ID;
            public User(int id) => ID = id;
        }

        private readonly struct Users
        {
            public readonly Entries<User> Entries;
            public Users(InsertMode inser, JournalMode journal)
            {
                Entries = new Entries<User>(inser, journal);
            }

            public void Add(int index) => Entries.Add(new Identity<User>(index), new User(index));
            public void Remove(int index) => Entries.Remove(new Identity<User>(index));
            public void AssertIds(params int[] exceptedIds)
            {
                var ids = Entries.Ids.Select(id => id.Value).ToArray();
                if (ids.Length != exceptedIds.Length)
                {
                    Assert.Fail($"excepted {exceptedIds.Length} ids count but have {ids.Length}");
                    return;
                }
                var passed = true;
                for (var i = 0; i < ids.Length; i++)
                {
                    if (exceptedIds[i] != ids[i])
                    {
                        passed = false;
                        break;
                    }
                }
                if (!passed)
                {
                    Assert.Fail($"ids not same. excepted {string.Join(",", exceptedIds)} but have {string.Join(",", ids)}");
                }
            }
        }

        [Test]
        public void EntriesTailNever()
        {
            var users = new Users(InsertMode.Tail, JournalMode.Never);

            users.Add(1);
            users.Add(2);
            users.Add(3);
            Assert.AreEqual(3, users.Entries.Count);
            users.AssertIds(1, 2, 3);

            users.Remove(2);
            Assert.AreEqual(2, users.Entries.Count);
            users.AssertIds(1, 3);

            users.Add(4);
            Assert.AreEqual(3, users.Entries.Count);
            users.AssertIds(1, 3, 4);

            users.Entries.Clear();
            Assert.AreEqual(0, users.Entries.Count);
            Assert.AreEqual(0, users.Entries.EmptyCellsCount);

            users.Add(1);
            users.Add(2);
            users.Add(3);
            users.Add(4);
            users.AssertIds(1, 2, 3, 4);
            foreach (var u in users.Entries) {
                if (u.ID == 1)
                {
                    users.Add(5);
                    Assert.AreEqual(5, users.Entries.Count);
                    users.AssertIds(1, 2, 3, 4, 5);
                }
                if (u.ID == 2)
                {
                    users.Remove(4);
                    Assert.AreEqual(4, users.Entries.Count);
                    Assert.AreEqual(1, users.Entries.EmptyCellsCount);
                    users.AssertIds(1, 2, 3, 5);
                }
                if (u.ID == 3)
                {
                    users.Add(6);
                    Assert.AreEqual(5, users.Entries.Count);
                    users.AssertIds(1, 2, 3, 5, 6);
                }
                if (u.ID == 4)
                {
                    Assert.Fail("User(4) removed");
                }
                Assert.AreEqual(0, users.Entries.JournalSize);
            }

            users.AssertIds(1, 2, 3, 5, 6);
            Assert.AreEqual(5, users.Entries.Count);
            Assert.AreEqual(0, users.Entries.EmptyCellsCount);
            Assert.AreEqual(0, users.Entries.JournalSize);

            users.Remove(1);
            users.Add(1);
            users.AssertIds(2, 3, 5, 6, 1);
        }

        [Test]
        public void EntriesMixedNever()
        {
            var users = new Users(InsertMode.Mixed, JournalMode.Never);

            users.Add(1);
            users.Add(2);
            users.Add(3);
            users.Remove(2);
            Assert.AreEqual(1, users.Entries.EmptyCellsCount);

            users.Add(4);
            users.AssertIds(1, 4, 3);

            users.Entries.Clear();
            Assert.AreEqual(0, users.Entries.Count);
            Assert.AreEqual(3, users.Entries.EmptyCellsCount);

            users = new Users(InsertMode.Mixed, JournalMode.Never);
            users.Add(1);
            users.Add(2);
            users.Add(3);
            users.Add(4);
            users.Add(5);
            foreach (var u in users.Entries)
            {
                if (u.ID == 1)
                {
                    users.Remove(3);
                    users.AssertIds(1, 2, 4, 5);
                    Assert.AreEqual(1, users.Entries.EmptyCellsCount);
                }
                if (u.ID == 2)
                {
                    users.Add(6);
                    users.Add(7);
                    users.AssertIds(1, 2, 4, 5, 6, 7);
                    Assert.AreEqual(1, users.Entries.EmptyCellsCount);
                }
                Assert.AreEqual(0, users.Entries.JournalSize);
            }

            users.AssertIds(1, 2, 4, 5, 6, 7);
            Assert.AreEqual(6, users.Entries.Count);
            Assert.AreEqual(1, users.Entries.EmptyCellsCount);

            users.Remove(1);
            users.Remove(2);
            users.Add(1);
            users.Add(2);
            users.AssertIds(2, 1, 4, 5, 6, 7);
        }

        [Test]
        public void EntriesTailOnRead()
        {
            var users = new Users(InsertMode.Tail, JournalMode.OnRead);

            users.Add(1);
            users.Add(2);
            users.Add(3);
            Assert.AreEqual(3, users.Entries.Count);
            Assert.AreEqual(0, users.Entries.JournalSize);
            users.AssertIds(1, 2, 3);

            foreach (var u in users.Entries)
            {
                if (u.ID == 1)
                {
                    users.Remove(1);
                    Assert.AreEqual(3, users.Entries.Count);
                    Assert.AreEqual(0, users.Entries.EmptyCellsCount);
                    Assert.AreEqual(1, users.Entries.JournalSize);
                    users.AssertIds(1, 2, 3);
                }
                if (u.ID == 2)
                {
                    users.Add(4);
                    users.Add(5);
                    Assert.AreEqual(3, users.Entries.Count);
                    Assert.AreEqual(0, users.Entries.EmptyCellsCount);
                    Assert.AreEqual(3, users.Entries.JournalSize);
                    users.AssertIds(1, 2, 3);
                }
                if (u.ID > 3)
                {
                    Assert.Fail("User with id > 3 not possible");
                }
            }
            Assert.AreEqual(4, users.Entries.Count);
            Assert.AreEqual(0, users.Entries.EmptyCellsCount);
            Assert.AreEqual(0, users.Entries.JournalSize);
            users.AssertIds(2, 3, 4, 5);

            users.Remove(3);
            users.Add(3);
            users.AssertIds(2, 4, 5, 3);
        }

        [Test]
        public void EntriesMixedOnRead()
        {
            var users = new Users(InsertMode.Mixed, JournalMode.OnRead);

            users.Add(1);
            users.Add(2);
            users.Add(3);

            foreach (var u in users.Entries)
            {
                if (u.ID == 1)
                {
                    users.Remove(1);
                    Assert.AreEqual(1, users.Entries.JournalSize);
                }
                if (u.ID == 2)
                {
                    users.Add(4);
                    Assert.AreEqual(2, users.Entries.JournalSize);
                }
                if (u.ID == 3)
                {
                    users.Add(5);
                    Assert.AreEqual(3, users.Entries.JournalSize);
                }
                if (u.ID > 3)
                {
                    Assert.Fail("User with id > 3 not possible");
                }
                users.AssertIds(1, 2, 3);
            }
            Assert.AreEqual(4, users.Entries.Count);
            Assert.AreEqual(0, users.Entries.EmptyCellsCount);
            Assert.AreEqual(0, users.Entries.JournalSize);
            users.AssertIds(4, 2, 3, 5);

            users.Remove(2);
            users.Add(2);
            users.AssertIds(4, 2, 3, 5);
        }

        [Test]
        public void EntriesTailAllways()
        {
            var users = new Users(InsertMode.Tail, JournalMode.Allways);

            users.Add(1);
            users.Add(2);
            users.Add(3);
            users.Remove(2);
            users.Add(4);
            Assert.AreEqual(0, users.Entries.Count);
            Assert.AreEqual(5, users.Entries.JournalSize);

            users.Entries.ApplyJournal();
            Assert.AreEqual(3, users.Entries.Count);
            Assert.AreEqual(0, users.Entries.JournalSize);
            users.AssertIds(1, 3, 4);

            users.Remove(3);
            users.Add(3);
            users.AssertIds(1, 3, 4);
            users.Entries.ApplyJournal();
            users.AssertIds(1, 4, 3);
        }

        [Test]
        public void EntriesMixedAllways()
        {
            var users = new Users(InsertMode.Mixed, JournalMode.Allways);

            users.Add(1);
            users.Add(2);
            users.Add(3);
            users.Remove(2);
            users.Add(4);
            users.Add(5);
            Assert.AreEqual(0, users.Entries.Count);
            Assert.AreEqual(6, users.Entries.JournalSize);

            users.Entries.ApplyJournal();
            Assert.AreEqual(4, users.Entries.Count);
            Assert.AreEqual(0, users.Entries.JournalSize);
            users.AssertIds(1, 4, 3, 5);

            users.Remove(1);
            users.Remove(3);
            users.Add(1);
            users.Add(3);
            users.AssertIds(1, 4, 3, 5);
            users.Entries.ApplyJournal();
            users.AssertIds(3, 4, 1, 5);
        }
    }
}