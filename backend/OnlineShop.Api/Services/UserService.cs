using MongoDB.Bson;
using OnlineShop.Api.Models;
using OnlineShop.Api.Repositories;

namespace OnlineShop.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    // ---------------------------------------------------------
    // VALIDATION HELPERS
    // ---------------------------------------------------------

    private static bool IsValidObjectId(string id)
    {
        return !string.IsNullOrWhiteSpace(id)
            && ObjectId.TryParse(id, out _);
    }

    private static bool IsValidRole(string role)
    {
        return !string.IsNullOrWhiteSpace(role);
    }

    private static bool IsValidUser(User user)
    {
        if (user == null)
            return false;

        if (string.IsNullOrWhiteSpace(user.Email))
            return false;

        return true;
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    public async Task<List<User>> GetAllAsync()
    {
        return await _repo.GetAllAsync();
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    public async Task<User?> GetByIdAsync(
        string id,
        string currentUserId,
        bool isAdmin)
    {
        if (!IsValidObjectId(id))
            return null;

        var user = await _repo.GetByIdAsync(id);

        if (user == null)
            return null;

        if (!isAdmin && currentUserId != id)
            return null;

        return user;
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    public async Task<User?> CreateAsync(User user)
    {
        if (!IsValidUser(user))
            return null;

        user.Email = user.Email.Trim();

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            return null;

        // Generate a valid Mongo ObjectId when necessary.
        if (string.IsNullOrWhiteSpace(user.Id) ||
            !IsValidObjectId(user.Id))
        {
            user.Id = ObjectId.GenerateNewId().ToString();
        }

        // Prevent duplicate email addresses.
        var allUsers = await _repo.GetAllAsync();

        var emailExists = allUsers.Any(existingUser =>
            !string.IsNullOrWhiteSpace(existingUser.Email) &&
            existingUser.Email.Equals(
                user.Email,
                StringComparison.OrdinalIgnoreCase));

        if (emailExists)
            return null;

        // Ensure every new user has the default role.
        if (user.Roles == null || user.Roles.Count == 0)
        {
            user.Roles = new List<string> { "User" };
        }
        else
        {
            // Clean supplied roles.
            user.Roles = user.Roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // If all supplied roles were empty/whitespace,
            // fall back to the default User role.
            if (user.Roles.Count == 0)
                user.Roles = new List<string> { "User" };
        }

        // Initialize timestamps only when they haven't
        // already been supplied.
        if (user.CreatedAt == default)
            user.CreatedAt = DateTime.UtcNow;

        if (user.UpdatedAt == default)
            user.UpdatedAt = user.CreatedAt;

        // IMPORTANT:
        // Do not overwrite IsEmailVerified here.
        //
        // User defaults it to false, but an explicitly supplied
        // true value must be preserved.

        await _repo.CreateAsync(user);

        return user;
    }

    // ---------------------------------------------------------
    // ADD ROLE
    // ---------------------------------------------------------

    public async Task<bool> AddRoleAsync(
        string userId,
        string role)
    {
        if (!IsValidObjectId(userId))
            return false;

        if (!IsValidRole(role))
            return false;

        role = role.Trim();

        var existing = await _repo.GetByIdAsync(userId);

        if (existing == null)
            return false;

        var existingRoles = existing.Roles ?? new List<string>();

        // Prevent duplicate roles.
        if (existingRoles.Any(existingRole =>
            existingRole.Equals(
                role,
                StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return await _repo.AddRoleAsync(userId, role);
    }

    // ---------------------------------------------------------
    // UPDATE PROFILE
    // ---------------------------------------------------------

    public async Task<User?> UpdateProfileAsync(
        string id,
        string fullName,
        string email,
        string? phoneNumber,
        Address? shippingAddress,
        Address? billingAddress)
    {
        if (!IsValidObjectId(id))
            return null;

        if (string.IsNullOrWhiteSpace(fullName))
            return null;

        if (string.IsNullOrWhiteSpace(email))
            return null;

        fullName = fullName.Trim();
        email = email.Trim();

        var existing = await _repo.GetByIdAsync(id);

        if (existing == null)
            return null;

        // -----------------------------------------------------
        // Email uniqueness
        // -----------------------------------------------------

        var allUsers = await _repo.GetAllAsync();

        var emailAlreadyExists = allUsers.Any(user =>
            user.Id != id &&
            !string.IsNullOrWhiteSpace(user.Email) &&
            user.Email.Equals(
                email,
                StringComparison.OrdinalIgnoreCase));

        if (emailAlreadyExists)
            return null;

        // -----------------------------------------------------
        // Email verification
        // -----------------------------------------------------

        var emailChanged = !string.Equals(
            existing.Email,
            email,
            StringComparison.OrdinalIgnoreCase);

        // -----------------------------------------------------
        // Preserve immutable fields
        // -----------------------------------------------------

        var originalCreatedAt = existing.CreatedAt;
        var originalPasswordHash = existing.PasswordHash;

        // -----------------------------------------------------
        // Basic profile fields
        // -----------------------------------------------------

        existing.FullName = fullName;
        existing.Email = email;

        // Password is intentionally NOT changed here.
        existing.PasswordHash = originalPasswordHash;

        // CreatedAt is immutable.
        existing.CreatedAt = originalCreatedAt;

        // -----------------------------------------------------
        // Phone number
        // -----------------------------------------------------

        // null means "not supplied" -> preserve existing value.
        //
        // whitespace means "explicitly clear" -> set null.
        if (phoneNumber != null)
        {
            existing.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber)
                ? null
                : phoneNumber.Trim();
        }

        // -----------------------------------------------------
        // Shipping address
        // -----------------------------------------------------

        // null means "not supplied" -> preserve existing address.
        if (shippingAddress != null)
        {
            existing.ShippingAddress = shippingAddress;
        }

        // -----------------------------------------------------
        // Billing address
        // -----------------------------------------------------

        // null means "not supplied" -> preserve existing address.
        if (billingAddress != null)
        {
            existing.BillingAddress = billingAddress;
        }

        // -----------------------------------------------------
        // Email verification
        // -----------------------------------------------------

        // Changing email requires verification again.
        if (emailChanged)
        {
            existing.IsEmailVerified = false;
        }

        // -----------------------------------------------------
        // Updated timestamp
        // -----------------------------------------------------

        existing.UpdatedAt = DateTime.UtcNow;

        // -----------------------------------------------------
        // Persist
        // -----------------------------------------------------

        var updated = await _repo.UpdateAsync(existing);

        if (!updated)
            return null;

        return existing;
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    public async Task<bool> DeleteAsync(string id)
    {
        if (!IsValidObjectId(id))
            return false;

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
            return false;

        return await _repo.DeleteAsync(id);
    }
}
