tp.Test("Collection should be created or already exist.", () =>
{
    int statusCode = (int)tp.Response.StatusCode;
    True(statusCode == 201 || statusCode == 409);
});
