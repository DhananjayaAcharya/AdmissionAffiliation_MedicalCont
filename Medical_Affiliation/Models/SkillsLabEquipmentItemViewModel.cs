namespace Medical_Affiliation.Models
{
    public class SkillsLabEquipmentItemViewModel
    {
        public int Id { get; set; }          // DB Id (optional)
        public string Name { get; set; } = "";
        public bool IsRequired { get; set; } // from NMC list
        public bool IsAvailable { get; set; }
        public int? Quantity { get; set; }
    }

    public class SkillsLabEquipmentViewModel
    {
        public IList<SkillsLabEquipmentItemViewModel> Items { get; set; }
            = new List<SkillsLabEquipmentItemViewModel>();

        public bool? HasTrainingModulesForAllModels { get; set; }
        public bool? UsesHybridModelsOrSimulations { get; set; }
        public bool? HasComputerAssistedLearningSpace { get; set; }
    }

}
