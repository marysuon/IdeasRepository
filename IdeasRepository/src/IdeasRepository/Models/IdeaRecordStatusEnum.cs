namespace IdeasRepository.Models
{
    public enum IdeaRecordStatusEnum : byte
    {
        //Created
        Created = 0,

        //Removed by Admin, requires confirmation
        RemovedByAdmin = 1,

        //Removed by User, requires confirmation
        RemovedByUser = 2,

        //Removed by Admin, confirmation denied(could be restored)
        ArchiveByUser = 3,

        //Removed by User, confirmation denied(couldn't be restored)
        ArchiveByAdmin = 4
    }
}
