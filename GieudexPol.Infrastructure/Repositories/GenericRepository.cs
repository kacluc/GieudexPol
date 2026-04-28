using GieudexPol.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.Infrastructure.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        // W celach demonstracyjnych, używamy prostej listy jako in-memory bazy danych.
        // W rzeczywistej aplikacji użyto by tutaj kontekstu bazy danych (np. Entity Framework).
        protected readonly List<T> _data = new List<T>();
        private int _nextId = 1;

        public async Task<T> GetByIdAsync(int id)
        {
            // W prawdziwej implementacji użylibyśmy LINQ do filtrowania po ID.
            // Tutaj symulujemy to przez Find dla prostych obiektów.
            return await Task.FromResult(_data.Find(item => (int)item.GetType().GetProperty("Id").GetValue(item) == id));
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Task.FromResult(_data);
        }

        public async Task AddAsync(T entity)
        {
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty != null && (int)idProperty.GetValue(entity) == 0)
            {
                idProperty.SetValue(entity, _nextId++);
            }
            _data.Add(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(T entity)
        {
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty == null)
            {
                throw new InvalidOperationException("Entity does not have an 'Id' property.");
            }

            var id = (int)idProperty.GetValue(entity);
            var existingEntity = _data.Find(item => (int)item.GetType().GetProperty("Id").GetValue(item) == id);
            if (existingEntity != null)
            {
                _data[_data.IndexOf(existingEntity)] = entity;
            }
            else
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(T entity)
        {
            _data.Remove(entity);
            await Task.CompletedTask;
        }
    }
}