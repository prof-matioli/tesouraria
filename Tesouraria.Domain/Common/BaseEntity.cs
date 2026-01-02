// Tesouraria.Domain/Common/BaseEntity.cs
using System;

namespace Tesouraria.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }
        public bool Ativo { get; set; } = true; // Soft Delete
    }
}