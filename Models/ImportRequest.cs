namespace brokenaccesscontrol.Models;

// A08 - Software and Data Integrity Failures.
// "Restaurar backup de preferências": payload JSON com tipo embutido, desserializado
// com TypeNameHandling.All (gadget). Ver ImportController + FileLog.
public class ImportRequest
{
    public string? Backup { get; set; }   // JSON serializado com $type
}
