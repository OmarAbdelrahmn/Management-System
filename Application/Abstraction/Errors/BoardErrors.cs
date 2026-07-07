using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class BoardErrors
{
    public static readonly Error NotFound = new("Board.NotFound", "Board was not found.", StatusCodes.Status404NotFound);
    public static readonly Error CycleNotFound = new("Board.CycleNotFound", "Board cycle was not found.", StatusCodes.Status404NotFound);
    public static readonly Error CycleTooLong = new("Board.CycleTooLong", "Board cycle duration cannot exceed one year.", StatusCodes.Status400BadRequest);
    public static readonly Error TooManyConsecutiveCycles = new("Board.TooManyConsecutiveCycles", "Consecutive cycle count cannot exceed 4.", StatusCodes.Status400BadRequest);
    public static readonly Error Closed = new("Board.Closed", "Board is closed and cannot be changed.", StatusCodes.Status409Conflict);
}
