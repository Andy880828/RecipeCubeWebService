using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RecipeCubeWebService.Models;

public partial class RecipeCubeContext : DbContext
{
    public RecipeCubeContext(DbContextOptions<RecipeCubeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<coupons> coupons { get; set; }

    public virtual DbSet<exclusive_ingredients> exclusive_ingredients { get; set; }

    public virtual DbSet<ingredients> ingredients { get; set; }

    public virtual DbSet<inventory> inventory { get; set; }

    public virtual DbSet<order_items> order_items { get; set; }

    public virtual DbSet<orders> orders { get; set; }

    public virtual DbSet<pantry_management> pantry_management { get; set; }

    public virtual DbSet<prefered_ingredients> prefered_ingredients { get; set; }

    public virtual DbSet<product_evaluate> product_evaluate { get; set; }

    public virtual DbSet<products> products { get; set; }

    public virtual DbSet<recipe_ingredients> recipe_ingredients { get; set; }

    public virtual DbSet<recipes> recipes { get; set; }

    public virtual DbSet<user> user { get; set; }

    public virtual DbSet<user_coupons> user_coupons { get; set; }

    public virtual DbSet<user_groups> user_groups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<coupons>(entity =>
        {
            entity.HasKey(e => e.couponId).HasName("PRIMARY");

            entity.Property(e => e.couponName).HasMaxLength(100);
            entity.Property(e => e.discountType).HasMaxLength(50);
            entity.Property(e => e.status).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<exclusive_ingredients>(entity =>
        {
            entity.HasKey(e => e.exclusiveIngredientId).HasName("PRIMARY");

            entity.HasIndex(e => e.userId, "idx_user_id");

            entity.Property(e => e.userId).HasMaxLength(450);
        });

        modelBuilder.Entity<ingredients>(entity =>
        {
            entity.HasKey(e => e.ingredientId).HasName("PRIMARY");

            entity.Property(e => e.category).HasMaxLength(100);
            entity.Property(e => e.gram).HasPrecision(10);
            entity.Property(e => e.ingredientName).HasMaxLength(255);
            entity.Property(e => e.photo).HasMaxLength(255);
            entity.Property(e => e.synonym).HasMaxLength(255);
            entity.Property(e => e.unit).HasMaxLength(10);
        });

        modelBuilder.Entity<inventory>(entity =>
        {
            entity.HasKey(e => e.inventoryId).HasName("PRIMARY");

            entity.HasIndex(e => e.ingredientId, "idx_ingredient");

            entity.Property(e => e.expiryDate).HasColumnType("date");
            entity.Property(e => e.quantity).HasPrecision(10);
            entity.Property(e => e.userId).HasMaxLength(450);
        });

        modelBuilder.Entity<order_items>(entity =>
        {
            entity.HasKey(e => e.orderItemId).HasName("PRIMARY");
        });

        modelBuilder.Entity<orders>(entity =>
        {
            entity.HasKey(e => e.orderId).HasName("PRIMARY");

            entity.HasIndex(e => e.userId, "idx_user_id");

            entity.Property(e => e.orderAddress).HasMaxLength(255);
            entity.Property(e => e.orderEmail).HasMaxLength(100);
            entity.Property(e => e.orderName).HasMaxLength(50);
            entity.Property(e => e.orderPhone).HasMaxLength(20);
            entity.Property(e => e.orderRemark).HasColumnType("text");
            entity.Property(e => e.orderTime).HasMaxLength(6);
            entity.Property(e => e.userId).HasMaxLength(450);
        });

        modelBuilder.Entity<pantry_management>(entity =>
        {
            entity.HasKey(e => e.pantryId).HasName("PRIMARY");

            entity.HasIndex(e => e.groupId, "idx_group");

            entity.HasIndex(e => e.userId, "idx_user");

            entity.Property(e => e.action).HasMaxLength(50);
            entity.Property(e => e.ownerId).HasMaxLength(450);
            entity.Property(e => e.quantity).HasPrecision(10);
            entity.Property(e => e.time)
                .HasMaxLength(6)
                .HasDefaultValueSql("'CURRENT_TIMESTAMP(6)'");
            entity.Property(e => e.userId).HasMaxLength(450);
        });

        modelBuilder.Entity<prefered_ingredients>(entity =>
        {
            entity.HasKey(e => e.preferIngredientId).HasName("PRIMARY");

            entity.Property(e => e.userId).HasMaxLength(450);
        });

        modelBuilder.Entity<product_evaluate>(entity =>
        {
            entity.HasKey(e => e.evaluateId).HasName("PRIMARY");

            entity.HasIndex(e => e.productId, "idx_product");

            entity.HasIndex(e => e.userId, "idx_user");

            entity.Property(e => e.commentMessage).HasMaxLength(1000);
            entity.Property(e => e.commentStars).HasDefaultValueSql("'5'");
            entity.Property(e => e.date)
                .HasDefaultValueSql("'curdate()'")
                .HasColumnType("date");
            entity.Property(e => e.userId).HasMaxLength(450);
        });

        modelBuilder.Entity<products>(entity =>
        {
            entity.HasKey(e => e.productId).HasName("PRIMARY");

            entity.Property(e => e.description).HasMaxLength(1000);
            entity.Property(e => e.photo).HasMaxLength(255);
            entity.Property(e => e.productName).HasMaxLength(255);
            entity.Property(e => e.unitQuantity).HasPrecision(10);
        });

        modelBuilder.Entity<recipe_ingredients>(entity =>
        {
            entity.HasKey(e => e.recipeIngredientId).HasName("PRIMARY");

            entity.Property(e => e.quantity).HasPrecision(10);
        });

        modelBuilder.Entity<recipes>(entity =>
        {
            entity.HasKey(e => e.recipeId).HasName("PRIMARY");

            entity.Property(e => e.category).HasMaxLength(10);
            entity.Property(e => e.description).HasMaxLength(255);
            entity.Property(e => e.detailedCategory).HasMaxLength(255);
            entity.Property(e => e.photo).HasMaxLength(255);
            entity.Property(e => e.recipeName).HasMaxLength(255);
            entity.Property(e => e.seasoning).HasMaxLength(255);
            entity.Property(e => e.steps).HasColumnType("text");
            entity.Property(e => e.time).HasMaxLength(255);
            entity.Property(e => e.userId).HasMaxLength(450);
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).HasMaxLength(450);
            entity.Property(e => e.ConcurrencyStamp).HasColumnType("text");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.LockoutEnd).HasColumnType("datetime");
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.PasswordHash).HasColumnType("text");
            entity.Property(e => e.PhoneNumber).HasColumnType("text");
            entity.Property(e => e.SecurityStamp).HasColumnType("text");
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        modelBuilder.Entity<user_coupons>(entity =>
        {
            entity.HasKey(e => e.userCouponId).HasName("PRIMARY");

            entity.Property(e => e.acquireDate).HasColumnType("date");
            entity.Property(e => e.status).HasDefaultValueSql("'1'");
            entity.Property(e => e.userId).HasMaxLength(450);
        });

        modelBuilder.Entity<user_groups>(entity =>
        {
            entity.HasKey(e => e.groupId).HasName("PRIMARY");

            entity.Property(e => e.groupAdmin).HasMaxLength(450);
            entity.Property(e => e.groupName).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
