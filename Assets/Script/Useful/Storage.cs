using System;
using System.Collections.Generic;

public class Storage : Singleton<Storage>
{
	private Dictionary<Type, List<object>> _storage = new();

	public void Register<T>(T member) where T : class
	{
		if (member == null)
			return;

		List<T> group = GetGroupOfType<T>();
		if (!group.Contains(member))
		{
			group.Add(member);
		}
	}

	public void Delete<T>(T member) where T : class
	{
		if (member == null)
			return;

		List<T> group = GetGroupOfType<T>();
		group.Remove(member);
	}

	public void DeleteFromAllGroups(object member)
	{
		if (member == null)
			return;

		Type memberType = member.GetType();
		foreach (var key in new List<Type>(_storage.Keys))
		{
			if (key.IsAssignableFrom(memberType))
			{
				_storage[key].Remove(member);
			}
		}
	}

	public void ClearStorage()
	{
		_storage.Clear();
	}

	public void ClearSpecificStorage<T>() where T : class
	{
		_storage.Remove(typeof(T));
	}

	public T GetElement<T>(int index = 0) where T : class
	{
		List<T> list = GetGroupOfType<T>();

		if (list.Count >= index) return null;

		return GetGroupOfType<T>()[index];
	}

	public List<T> GetGroupOfType<T>() where T : class
	{
		if (!_storage.TryGetValue(typeof(T), out List<object> group))
		{
			group = new List<object>();
			_storage[typeof(T)] = group;
		}

		return group.ConvertAll(item => (T)item);
	}
}