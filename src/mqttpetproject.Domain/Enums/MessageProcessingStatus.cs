namespace mqttpetproject.Domain.Enums;

public enum MessageProcessingStatus
{
    Ack = 1,
    RejectToDlq = 2,
    NackRequeue = 3
}
