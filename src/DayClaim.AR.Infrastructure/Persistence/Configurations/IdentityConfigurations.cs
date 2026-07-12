using DayClaim.AR.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayClaim.AR.Infrastructure.Persistence.Configurations;

public class ClientOrganizationConfig : IEntityTypeConfiguration<ClientOrganization>
{
    public void Configure(EntityTypeBuilder<ClientOrganization> builder)
    {
        builder.ToTable("users_client_organizations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Size).HasConversion<string>().HasMaxLength(20);
    }
}

public class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("users_roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public class PermissionConfig : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("users_permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(150).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.Module).HasMaxLength(100);
    }
}

public class RolePermissionConfig : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("users_role_permissions");
        builder.HasKey(x => new { x.RoleId, x.PermissionId });
        builder.HasOne(x => x.Role).WithMany(r => r.RolePermissions).HasForeignKey(x => x.RoleId);
        builder.HasOne(x => x.Permission).WithMany().HasForeignKey(x => x.PermissionId);
    }
}

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users_users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Username).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.Username).IsUnique();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200);
    }
}

public class UserRoleConfig : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("users_user_roles");
        builder.HasKey(x => new { x.UserId, x.RoleId });
        builder.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
    }
}

public class UserOrganizationConfig : IEntityTypeConfiguration<UserOrganization>
{
    public void Configure(EntityTypeBuilder<UserOrganization> builder)
    {
        builder.ToTable("users_user_organizations");
        builder.HasKey(x => new { x.UserId, x.ClientOrganizationId });
        builder.HasOne(x => x.User).WithMany(u => u.UserOrganizations).HasForeignKey(x => x.UserId);
    }
}

public class UserMenuAccessConfig : IEntityTypeConfiguration<UserMenuAccess>
{
    public void Configure(EntityTypeBuilder<UserMenuAccess> builder)
    {
        builder.ToTable("users_menu_access");
        builder.HasKey(x => new { x.UserId, x.MenuPath });
        builder.Property(x => x.MenuPath).HasMaxLength(300).IsRequired();
        builder.HasOne(x => x.User).WithMany(u => u.MenuAccess).HasForeignKey(x => x.UserId);
    }
}

public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("users_refresh_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.Property(x => x.Jti).HasMaxLength(100);
    }
}
