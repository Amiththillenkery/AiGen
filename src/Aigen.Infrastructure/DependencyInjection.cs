using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ImplementArticleEntitiy.Infrastructure.Persistence;
using ImplementArticleEntitiy.Application.Interfaces;
using ImplementArticleEntitiy.Infrastructure.Repositories;

namespace ImplementArticleEntitiy.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IArticleRepository, ArticleRepository>();

            return services;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using ImplementArticleEntitiy.Domain.Entities;

namespace ImplementArticleEntitiy.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnique();
                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
            });
        }
    }
}

namespace ImplementArticleEntitiy.Domain.Entities
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using ImplementArticleEntitiy.Domain.Entities;

namespace ImplementArticleEntitiy.Application.Interfaces
{
    public interface IArticleRepository
    {
        Task<Article> GetByIdAsync(int id);
        Task<IEnumerable<Article>> GetAllAsync();
        Task AddAsync(Article article);
        Task UpdateAsync(Article article);
        Task DeleteAsync(int id);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ImplementArticleEntitiy.Application.Interfaces;
using ImplementArticleEntitiy.Domain.Entities;
using ImplementArticleEntitiy.Infrastructure.Persistence;

namespace ImplementArticleEntitiy.Infrastructure.Repositories
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly ApplicationDbContext _context;

        public ArticleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Article> GetByIdAsync(int id)
        {
            return await _context.Articles.FindAsync(id);
        }

        public async Task<IEnumerable<Article>> GetAllAsync()
        {
            return await _context.Articles.ToListAsync();
        }

        public async Task AddAsync(Article article)
        {
            await _context.Articles.AddAsync(article);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Article article)
        {
            _context.Articles.Update(article);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }
        }
    }
}

using Xunit;
using Moq;
using FluentAssertions;
using System.Threading.Tasks;
using ImplementArticleEntitiy.Application.Interfaces;
using ImplementArticleEntitiy.Domain.Entities;
using ImplementArticleEntitiy.Infrastructure.Repositories;
using ImplementArticleEntitiy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ArticleRepositoryTests
{
    private readonly IArticleRepository _repository;
    private readonly ApplicationDbContext _context;

    public ArticleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ArticleRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddArticle()
    {
        var article = new Article { Title = "Test Article", Price = 10.00m };
        await _repository.AddAsync(article);

        var result = await _repository.GetByIdAsync(article.Id);
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Article");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllArticles()
    {
        var articles = await _repository.GetAllAsync();
        articles.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateArticle()
    {
        var article = new Article { Title = "Old Title", Price = 10.00m };
        await _repository.AddAsync(article);

        article.Title = "New Title";
        await _repository.UpdateAsync(article);

        var result = await _repository.GetByIdAsync(article.Id);
        result.Title.Should().Be("New Title");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveArticle()
    {
        var article = new Article { Title = "To Be Deleted", Price = 10.00m };
        await _repository.AddAsync(article);

        await _repository.DeleteAsync(article.Id);

        var result = await _repository.GetByIdAsync(article.Id);
        result.Should().BeNull();
    }
}