using AnubotBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace AnubotBackend.Dto;

public class UpdateChatDto
{
    [EnumDataType(typeof(Feedback))]
    public Feedback Feedback { get; set; }
}
