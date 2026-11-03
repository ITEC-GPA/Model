"""Explicit review of the remaining F2 declarations. Imported by review_f5_matrix.

These are contract destinations, not assertions that F6 component migration has run.
An implementation-required row continues to fail the F5 coverage gate.
"""
def register(route, common):
    core='tests/MigrationCopyContractsTest.cs; tests/ElasticExportContractsTest.cs'
    physical='tests/PhysicalAssignmentsTest.cs; UnitTest/Straus7ModelWriteTest.cs'
    result='Model/Results; UnitTest/ResultArchiveTest.cs; UnitTest/Straus7ModelWriteTest.cs'
    checker='Checker/GPCChecker.CompositeBridge/Models/BridgeWorkflowDefinition.cs; Checker CompositeBridge tests'
    source='BridGHe/src/BridGHe.Common/Helpers/StrausR3Helper.cs'
    m='BridGHe.Common.Models.'
    a='Arachne.Common.Models.'
    common('ModelModel','Name','Model.Name','text',core)
    common('ModelModel','Elements Nodes Beams Plates _elements _nodes _beams _plates','Model element collections; typed filtered collections share graph nodes; ModelAssembly supplies deterministic solver IDs','N-mm-t-s-C internal; persistent Guids and model integer IDs are distinct',core)
    common('ModelModel','LoadCases _loadCases','Model.LoadCases; references are rebound to the assembled case graph','case Guid',core)
    common('ModelModel','_nodesIndexes _beamsIndexes _platesIndexes','Adapter lookup caches rebuilt from ModelAssembly/identity map; never persistent structural input','source string -> persistent Guid / assembly ID',core,'retain-plugin-cache')
    common('ModelModel','Units GRAVITY_mmSuS2','Converter boundary unit normalization; GPC internal N-mm-t-s-C, explicit gravity 9806.65 mm/s2 only when requested','source unit-set tag; dimensional conversion, never a relabel', 'docs/bridghe-arachne/F2-CONTRATTI.md; Model/Core/Units','retain-adapter')
    common('ModelModel+ModelUnits','Knm SI Nmm IPS','Converter named unit mapping; dimensional factors for every quantity, source tag retained in import provenance','kN-m, N-m, N-mm, inch-pound-second respectively; mass derived consistently','docs/bridghe-arachne/F2-CONTRATTI.md','retain-adapter')
    route(m+'ModelModel','LoadCombinations StagedConstruction MovingPaths GeneratedMovingCases BridgeWorkflow','Model.Combinations / Stages / MovingLoads; generated cases belong to Straus7ModelRun result identity map; BridgeWorkflowDefinition remains Checker-owned','stable case/element Guids; generated steps are result state',physical+'; '+checker)
    common('LoadCaseModel','Dead','Adapter convenience singleton -> explicit self-weight LoadCase; no process-wide shared mutable GPC case','case identity; gravity is explicit',core)
    route(m+'LoadCaseModel','Stages','Checker BridgeWorkflowPhaseDefinition.Actions; solver stages separately use Model.Stages.AnalysisState','phase/case Guids; no string join in computation',checker)
    route(m+'LoadCaseModel+Stage','Name BridgePhase','BridgeWorkflowPhaseDefinition.Name/Actions and BridgeBeamSectionState.Behavior; legacy phase labels normalized explicitly','names; named phase mapping',checker)
    route(m+'LoadCaseModel+Stage+BridgePhases','Unassigned OnlySteel NInfinite NInstant StrainInfinite StrainInstant NShrinkage','BridgeActionBehavior plus ForceCaseGuid/StrainCaseGuid and ModularRatio; Unassigned cannot silently enter a phase','enum; ratios dimensionless, strains dimensionless',checker)
    route(m+'LoadCombinationModel','Values','Combination case factors by registered case reference; no evaluated result stored as input','dimensionless signed coefficients','Model/Actions/Combinations/Combination.cs; Model/Results/Processing/CombinationMatrix.cs')
    route(m+'BeamReleasesAttributeModel+AttributeData','TransNode1 TransNode2 RotNode1 RotNode2 TransN1Partial TransN2Partial RotN1Partial RotN2Partial','BeamReleasesAttribute.I/J BeamDofConnection arrays: continuous/released/partial spring, section axes explicit','translation N/mm; rotation N mm/rad; source status enum mapped by name',physical)
    route(m+'BeamSupportAttributeModel+AttributeData','SpringType LinkProperty SpringTensionOrientation Plane12Angle LocalAxis2Angle CoordinateSystem','BeamDistributedSupport for Simple/BeamLocalAxis; unsupported LinkProperty/custom-plane inputs retained with diagnostic exactly as source BeamSupportExport','angles degrees at legacy boundary; signed section vector dimensionless','BridGHe/src/BridGHe.Common/Straus/BeamSupportExport.cs','reviewed-legacy-limit')
    route(m+'BeamSupportAttributeModel+CoordinateSystem','Global Local','Explicit coordinate system; source BeamSupportExport requires local support direction and rejects unsupported combinations','named coordinate basis','BridGHe/src/BridGHe.Common/Straus/BeamSupportExport.cs','reviewed-legacy-limit')
    route(m+'LoadPatchTypeAttributeModel+PatchType','Auto1 Auto2 Auto3 Auto4 AngleSplit Manual','ShellLoadPatch.Distribution named mapping; exact native choice retained','dimensionless algorithm selector',physical)
    route(m+'NodeDisplacementLoadModel+LoadData','Translation Rotation','RestrainAssignment.Case/FreedomCase + DofRestrain.Prescribe; staged increments explicitly pair freedom and load factors','translation mm; effective source rotation degrees -> GPC radians (old GH input did not label units); native results rad', 'UnitTest/Straus7ModelWriteTest.NativePrescribedTranslationAndRotationFollowLoadAndFreedomFactors')
    route(m+'NodeRestraintAttributeModel+AttributeData','TransationalDisplacement RotationDisplacement','Legacy declarations ignored by source exporter (which reads NodeDisplacementLoadModel); preserve in compatibility metadata and diagnose nonzero values before promotion','mm; legacy rotation convention ambiguous; no silent activation',source,'retain-compatibility-F6')
    common('PlateModel','Type','ShellGeometry topology derived from ordered connectivity; formulation declared independently','vertex count/order; never infer material formulation from mesh type',core)
    common('PlateModel+PlateType','Invalid Tri3 Quad4 Tri6 Quad8 Quad9 Generic','ShellGeometry Tri3/Quad4; higher-order/generic topology explicitly rejected by current GPC shell contract before export, retained in migration diagnostic','no corner-only truncation; high-order geometry requires a separate formulation','docs/bridghe-arachne/F2-CONTRATTI.md; Model/Structure/Topology/ShellGeometry.cs','reviewed-contract-limit')
    route(m+'PlatePressureModel+LoadFace','Top Bottom','ShellSurfacePressureFace.PositiveNormal / NegativeNormal with ordered face normal','positive pressure inward; source unsafe one-value API call cannot define a valid two-face contract',physical)
    route(m+'PlateSupportAttributeModel+SupportedFace','Top Bottom','ShellElasticSupport face and signed direction resolved from ordered normal','N/mm3, gap mm',physical)
    common('PlatePropertyModel','_id','Property Guid plus source-string identity map; backing field of Id','source string ID',core)
    common('ResultModel','_case','ResultState.CaseId; backing field of LoadCase in source','persistent case Guid',result)
    common('PlatePropertyModel','Thickness','ElasticPlateProperty.MembraneThickness and BendingThickness; source single thickness feeds both','mm',core)
    route(m+'PlateForceResultModel','Force Moment TransverseShearForce NodeId','PointResultPlateForces tensor components / explicit shell node location; AnalysisDataset normalized frame','N/mm; N mm/mm; transverse N/mm; source node mapped to Guid',result)
    route(m+'PlateForceResultModel','ForceMax ForceMin ForceAngle ForceVonMises MomentMax MomentMin MomentAngle TransverseShearForceMax TransverseShearForceAng','Principal tensor invariants / resultant transverse shear recomputed from the full tensor; preserve raw native table in dataset provenance for equivalence','principal force N/mm; moment N mm/mm; angles rad after source normalization',result)
    common('MaterialModel','Id Name','Material.Guid/Name and source identity map','text; not a native material index',core)
    common('MaterialModel','Kind','Typed SteelMaterial/ConcreteMaterial/SteelMaterialEN1992 with SteelTypes.Rebar/AluminiumMaterial or membrane Orthotropy; Undefined diagnoses missing physical type','named family mapping',core)
    common('MaterialModel','Modulus PoissonRatio Density ThermalExpansion','Material E/Ni/Density/AlfaThermalExpansion; isotropic input only, orthotropy independent','MPa, dimensionless, tonne/mm3, 1/C',core)
    common('MaterialModel','MinimumYieldStress MinimumTensileStress SpecificCompressiveStrength','SteelMaterial Fyk/Fu, concrete characteristic compressive strength; preserve material family and characteristic/design distinction','MPa; reinforcement uses its own characteristic steel law',core)
    common('MaterialModel','ExpectedYieldStress ExpectedTensileStress ExpectedCompressiveStrength CompressiveYieldStrength TensileYieldStrength TensileUltimateStrength ShearUltimateStrength Alloy AlumiumType','Material.Declarations: expected strengths, declared directional strengths, alloy and AluminiumForm; no implicit design/constitutive use','MPa (positive strength magnitudes); alloy text; manufacturing form is NOT GPC AluminiumTypes Structural/Bolt','tests/ElasticExportContractsTest.MaterialDeclarationsRetainValuesWithoutChangingConstitutiveResponse')
    common('MaterialModel+AlumiumTypes','Wrought CastMold CastSand','MaterialDeclarations.AluminiumForm named mapping','enum',core)
    common('MaterialModel+MaterialKind','Undefined Steel Concrete Aluminium Rebar Membrane','Typed material dispatch; membrane -> OrthotropicElasticity and MembranePlateProperty; Undefined rejected','named mapping; no enum ordinal cast',core)
    common('MaterialModel','TemperatureOnTime','Preserved legacy metadata; source enum is Elastic/Plastic, despite its name; no temperature/time law is inferred','no dimensional quantity; source exporter does not consume it',source,'retain-compatibility-F6')
    common('MaterialModel+TemperatureOnTimeType','Elastic Plastic','Preserved compatibility enum alongside TemperatureOnTime; not an independent-variable selector','enum',source,'retain-compatibility-F6')
    common('MaterialModel+MaterialMode','Elastic ElasticPlastic','MaterialAnalysisData.NonlinearResponse; source effective default must be compared before activating a stored flag','nonlinear elastic / elastoplastic','UnitTest/Straus7ModelWriteTest.cs')
    common('MaterialModel+HardeningType','Isotropic Kinematic Takeda','MaterialAnalysisData.Hardening named mapping; requires explicit stress-strain input','dimensionless constitutive selector','tests/ElasticExportContractsTest.cs')
    route(m+'MaterialModel','ReinforcementUltimateStrain','Reinforcement characteristic ultimate strain in SteelMaterialEN1992 with SteelTypes.Rebar; null preserves historical conversion default','dimensionless strain; not percent',core)
    route(m+'MaterialModel','_shearUltimateStrength','Material.Declarations.ShearUltimateStrength retains the ARCHIVED private value; legacy public getter incorrectly reads tensile ultimate strength, so F6 must report both before resolving conflict','MPa; no automatic substitution of tensile strength','BridGHe MaterialModel.cs getter vs GetObjectData; tests/ElasticExportContractsTest.cs')
    common('BeamPropertyModel','PropertyType','BeamAssignments.Formulation; named mapping, not native integer or enum ordinal cast','enum',physical)
    common('BeamPropertyType','PointContact Pipe Connection','BeamFormulation.PointContact/Pipe/Connection; native types 5/8/9, exact defaults retained; specialized checks require explicit compatible formulation','native section properties in N-mm; no additional contact/friction/pressure law inferred','UnitTest/Straus7ModelWriteTest.NativeAdditionalBeamFormulationsKeepTheirDeclaredType')
    common('BeamPropertyType','SpringDamper UserDefined','Source exporter has no mapping (creates btNull); reject as unsupported source formulation, retain diagnostic instead of silently creating a null beam','enum; no spring law or user routine exists on these source models',source,'reviewed-legacy-limit')
    route(m+'LoadCaseModel','GlobalInertiaLoad','None -> no model action; Gravity -> ModelGravityLoad; AccelerationAndVelocity/Seismic are inactive source declarations and require explicit diagnostic before activation','named selector; mass family selection retained separately',source)
    route(m+'LoadCaseModel+GlobalInertiaLoads','None Gravity AccelerationAndVelocity Seismic','ModelGravityLoad only for source Gravity; other named declarations retained with UnsupportedReason when present because source exporter did not apply them','no seismic spectrum or rotational inertia inferred',source,'retain-compatibility-F6')
    route(m+'LoadCaseModel','Temperature A V W Origin','PreservedAssignment records inactive source declarations with original unit-set tag; source exporter ignores these fields, so migration must not silently turn them into active thermal/inertial loads','Temperature C, Origin model length; A/V/W exact source vectors with UNRESOLVED physical convention (source has no consumer), no inferred rotational formula',source,'retain-compatibility-F6')
    route(m+'SectionModel','NativeSection','Straus7SectionDefinition + Straus7SectionCatalog; ResolveLibrary expands BSL/BGL, CaptureBxs embeds bytes; native assignment keyed by beam Guid, snapshot codec and result provenance explicit','mm; native section principal angle rad; original library/BXS paths are not required after resolution','UnitTest/Straus7SectionDefinitionTest.cs','converter-contract')
    # Old n0/n1 caches are only copied/serialized in current source, not used by its solver or checks.
    common('SectionModel','SectionArea_n0 SectionArea_n1 Ixx_n0 Ixx_n1 Iyy_n0 Iyy_n1 J_n0 J_n1 RebarArea RebarDistanceFromTop N0 N1','Compatibility metadata retains inactive legacy caches; active composite geometry/ratios come from CompositeBeamPropertyModel, not these obsolete SectionModel fields','mm2/mm4/mm; ratios dimensionless; preserve explicit values without recomputing from a guessed n0/n1 meaning','Source SectionModel declarations/GetObjectData/CopyFrom and repository consumers','retain-compatibility-F6')
    route(m+'SectionConcreteSlabModel','Height Width TopCover TopRebarArea BottomCover BottomRebarArea TotalRebarArea','ReinforcedConcreteSection rectangular slab and two RebarSectionCircular equivalent rows at their explicit centroids; TotalRebarArea derived from both rows','mm; area mm2; covers to equivalent-row centroid as source factory; no fictitious count of individual bars','Model/Sections/Concrete/ReinforcedConcreteSection.Composite.cs; BridGHe CompositeBeamPropertyModel.GetReinforcedConcreteSection')
    route(m+'CompositeBeamPropertyModel','ConcreteSection SteelSection ConcreteMaterial RebarMaterial SteelMaterial','ReinforcedConcreteSection with physical slab, SteelSectionPosition and reinforcement rows/materials; copied independently of verification state','mm, MPa; source defaults retained explicitly',core+'; Checker CompositeBridge tests')
    route(m+'CompositeBeamPropertyModel','VerticalOffset','SteelSectionPosition geometry could express a relative placement, but source CompositeSectionFactory explicitly rejects nonzero vertical offset; retain the same diagnostic until a separately verified geometry conversion exists','mm; never reinterpret as horizontal eccentricity','BridGHe/src/BridGHe.Common/Checks/CompositeSectionFactory.cs','reviewed-legacy-limit')
    route(m+'CompositeBeamPropertyModel','NInfinite NInstant NShrinkage _nShrinkage ConsiderConcrete ConsiderRebar','CompositeSectionDefinition.LongTermRatio/ShortTermRatio/ShrinkageRatio/IncludeConcrete/IncludeReinforcement; CompositeSectionProperties calculates on a detached physical section; Checker BridgeBeamSectionState binds the selected ratio to each action','dimensionless n=Es/Ec; source NShrinkage NaN -> nullable ShrinkageRatio with explicit long-term fallback','tests/ElasticExportContractsTest.CompositeDefinitionAndInclusionFlagsRetainPhysicalGeometryAndArchiveIndependently; Checker/BridgeMigrationPhysicsTests')
    route(m+'CompositeBeamSlabStrainModel+LoadData','Strain','BridgeWorkflowActionDefinition.Strain and its StrainCaseGuid; Checker calculates section eigen-actions; physical equivalent loads only through an explicit conversion','dimensionless free slab strain, not whole-beam axial strain',checker)
    route(m+'CompositeBeamModel','BeamId PointFrom PointTo Length','BeamElement identity/end nodes; physical member path and length derived from connected geometry','mm; source ID mapped, persistent Guid assigned',core)
    route(m+'CompositeBeamModel','StartBeamProperty EndBeamProperty','BeamSectionAssignment.Property/EndProperty retain physical composite endpoints; tabulated section samples or explicit geometric interpolation must retain the source component sampling rule; exporter requires subdivision for nonconstant schedules','station normalized on the physical member; no unsupported interpolation silently selected','Model/Structure/Assignments/SectionLaws.cs; PIANO.md F2 explicit limits')
    route(a+'BeamNonPrismaticPropertyModel','EndSection','BeamSectionAssignment.EndProperty with mapped ElasticBeamProperty.Section; source numerical form-finding does not consume this endpoint (preview only)','mm section geometry; no inferred force-density effect',core)
    route(a+'BeamNonPrismaticPropertyModel','Length LengthType Inertia33Type Inertia22Type','Preserve source nonprismatic preview/compatibility metadata; no source solver consumer of these interpolation selectors; any future rigidity interpolation must have an explicit supported law','source Length retained with its type tag; linear/parabolic/cubic are labels, no polynomial formula inferred','Arachne BeamNonPrismaticPropertyModel and BeamModel preview consumers','retain-compatibility-F6')
    route(a+'BeamNonPrismaticPropertyModel+LengthTypes','Variable Absolute','Retained nonprismatic source length selector; used only by compatibility/preview adapter','enum','Arachne source consumers','retain-compatibility-F6')
    route(a+'BeamNonPrismaticPropertyModel+InertiaTypes','Linear Parabolic Cubic','Retained source interpolation selector; not equated to GPC geometric SectionVariation','enum','Arachne source consumers','retain-compatibility-F6')
    route(a+'BeamNonPrismaticPropertyModel','EndSectionSectionArea EndSectionIxx EndSectionIyy EndSectionIxy EndSectionI11 EndSectionI22 EndSectionJ EndSectionW11 EndSectionW22 EndSectionRho11 EndSectionRho22 EndSectionWxx EndSectionWyy EndSectionAngleX1Rad EndSectionShearL1 EndSectionShearL2 EndSectionShearA1 EndSectionShearA2 EndSectionIntegrationPoints EndSectionSectionType EndSectionD EndSectionB EndSectionB1 EndSectionB2 EndSectionL EndSectionT EndSectionT1 EndSectionT2 EndSectionT3 EndSectionMirror EndSectionMirrorGapA EndSectionMirrorGapB EndSectionCentroid EndSectionIncludeTorsionalStiffness','Derived getters of EndSection: same field mapping as SectionModel, including principal-axis conventions, shear offsets and expanded mirrored geometry; no independent stored values','mm powers per SectionModel; principal angle rad; Rho11=sqrt(I11/A), Rho22=sqrt(I22/A)',core)
    ff='tests/FormFindingContractsTest.cs; docs/bridghe-arachne/F4-PILOTI.md'
    a='Arachne.Common.Models.'
    route(a+'FixedAxialForceModel','_weight','FormFindingGoal.Weight','dimensionless equation weight, not mass',ff)
    route(a+'FixedLengthModel','_weight','FormFindingGoal.Weight','dimensionless equation weight',ff)
    route(a+'ForceDensityModel','_weight _curvatureRatioValue _type _membraneType _project','FormFindingGoal Weight/Target/Kind/Direction/ProjectToPlane; chord explicitly retained for CurvatureRatio','weight and curvature ratio dimensionless; force density N/mm',ff)
    route(a+'ForceDensityModel+ForceDensityTypes','ForceDensity Cable Membrane Curvature','FormFindingGoalKind ForceDensity/CableForce/MembraneForcePerWidth/CurvatureRatio','N/mm, N, N/mm, dimensionless respectively',ff)
    route(a+'ForceDensityModel+MembraneTypes','Undefined Warp Weft','FabricDirection.Unspecified/Warp/Weft','named mapping; fabric direction applies only to membrane objectives',ff)
    route(a+'SolverOptionsModel','Gravity Pressure MaxIterations Tolerance SolverMaxIterations SolverToleranceA SolverToleranceB DefaultSolverTolerance','FormFindingDefinition Gravity/Pressure/MaxIterations/RelativeTolerance/SolverMaxIterations/SolverToleranceA/B','mm/s2, N/mm2; dimensionless tolerances; explicit iteration counts',ff)
    route(a+'SolverOptionsModel','LoadCases PressureLoadCase','Adapter selection resolves applied GPC load cases before solving; output pressure case identified in the result dataset','case Guids; empty selection means all source loads',ff,'retain-adapter')
    route(a+'SolverOptionsModel','CreateMesh PlateProperty','Plugin meshing/output policy; generated GPC AreaElements have the mapped physical plate property and surface masses','boolean plus typed ElasticPlateProperty/MembranePlateProperty',ff,'retain-plugin-algorithm')

    # These objects are Rhino/UI orchestration products, not new structural domain classes.
    # Every contained structural value has its own explicit domain row in the matrix.
    for name,fields in {
        'BeamMomentSign':'Positive Negative Mixed Zero Missing',
        'BeamMomentState':'Source Sign Minimum Maximum Case',
        'BridgeAlignmentInputsResult':'Alignment Length Supports Spans StartOverhang EndOverhang Joints SupportPoints Information',
        'BridgeBearingKind':'FixedTranslation GuidedLongitudinal Multidirectional Elastic',
        'BridgeBearingLayout':'Nodes Frames Labels',
        'BridgeConstructionSnapshot':'Model Stage ActivePhysicalIds Diagnostics',
        'BridgeDeckStrip':'Member Width',
        'BridgeDeckConnection':'Station Girder Axis',
        'BridgeDeckGrillageResult':'Geometry Stations Strips Connections',
        'BridgeDeckResult':'Geometry Mesh Plates Connections Stations Diagnostics',
        'IBridgeSegments':'FabricationSegments',
        'BridgeJointSet':'Stations Girders',
        'BridgeDetailPlacement':'Id Girder Station Definition Kind',
        'BridgeDetailLayoutResult':'StudAxes StudHeads Stiffeners Details Diagnostics',
        'BridgeParametricLayoutResult':'Alignment Supports Offsets DeckWidths SpanLengths BearingTypes',
        'BridgeAssignment':'Beams Unassigned Diagnostics',
        'BridgeSegmentCompositeResult':'Girders Properties SegmentIds',
        'BridgeSegmentOverrideResult':'Girders OtherMembers',
        'BridgeSegmentSectionResult':'Rules Table ResolvedTable',
        'BridgeSlabGrillageResult':'Beams Widths',
        'Shapes':'_items Count PREFIX',
    }.items():
        route('BridGHe.Common.Geometry.'+name,fields,'Plugin algorithm/result wrapper; structural payload uses GPC BridgeDefinition/BridgeGeometry/BridgeSectionRule, Element/Stage and Checker detail definitions; Rhino display meshes stay local','geometry mm; moments N mm; labels text; no solver IDs inferred from UI labels','PIANO.md F5/F6; source declaration and contained types','retain-plugin-algorithm')
    route(m+'CompositeSectionPreviewGeometry','Steel Slab','Rhino preview geometry generated from the GPC physical sections; never serialized as an analysis property','mm','PIANO.md F6','retain-plugin-preview')
    route(m+'SectionModel','CompositePreview','Rhino preview wrapper generated from physical composite section','mm','PIANO.md F6','retain-plugin-preview')
    route('BridGHe.Common.MovingLoads.BridgeMovingCombinationTable','Rows ActionIds Cases','Plugin table wrapper around GPC Combination factors and stable moving action/case identities','dimensionless factors, identifiers text/Guid','Model/Actions/Moving; PIANO.md F6','retain-plugin')
    for name,fields in {
        'BridgeCheckAdapter':'section reference options PlanarAssumption LocalStateAssumption ignoreSecondaryActions',
        'BridgePhaseAssembler+ResultIndex':'BeamId _forces _strains _tolerance',
        'BridgeResultLabelFormat':'Complete BeamStation Combination Phase',
        'BridgeResultSource':'LocationLabel Label BeamId StationMm Combination SuperElement',
        'StiffenerPlotRole':'Intermediate Support',
        'CheckPlotDisplay':'Utilization Demand Resistance DemandAndResistance',
        'StiffenerPlotReading':'Value Limit Unit Governing Unavailable',
        'BridgeStiffenerPlotChecks':'MemberChecks',
        'BridgeStressResult':'Analysis Stage Source Assumptions stageIndex shear rebarRows Point1_ConcreteSigma Point1_ConcreteEpsilon Point2_RebarSigma Point3_RebarSigma Point4_ConcreteSigma Point5_SteelSigma Point6_SteelSigma Point7_SteelSigma Point6_SteelTau Point7_SteelTau Point6_SteelSigmaId Point7_SteelSigmaId Point8_SteelSigma Point8_SteelEpsilon',
        'BridgeWorkflowBuildResult':'Model Summary Warnings',
    }.items():
        route('BridGHe.Common.Checks.'+name,fields,'Plugin extraction/plot/diagnostic adapter over Checker CompositeBridge workflow/results; replace contained legacy model references in F6, calculations remain Checker-owned','MPa stresses, dimensionless strain/utilization, mm locations; Checker boundary explicitly converts N to kN and N mm to kN m',checker+'; PIANO.md F6','retain-plugin-adapter')
    for name,fields,target in (
        ('BridgePhaseActions','Steel Infinite Instant StrainInfinite StrainInstant Shrinkage','BridgeWorkflowActionDefinition Behavior/ForceCaseGuid/StrainCaseGuid/Strain'),
        ('BridgeActionBehavior','Steel LongTerm ShortTerm Shrinkage','Checker BridgeActionBehavior'),
        ('BridgeSectionState','Steel Composite ConcreteExcluded','Checker BridgeWorkflowSectionState'),
        ('BridgeWorkflowMethod','Compatible LinearHistory','Checker BridgeWorkflowMethod'),
        ('CompositeShrinkageActions','FreeStrain Curvature PreStrain ThermalGradient EigenForce EigenMoment Centroid','Checker CompositeShrinkageActions.Calculate returns named values; translate physical equivalent effects to BeamInitialState/BeamTemperatureGradient when explicitly requested; source record remains a presentation wrapper'),
        ('ConcreteShrinkageResult','Increment Total Drying Autogenous NotionalSize Kh','Checker ConcreteShrinkage.Calculate returns named computed values; source record remains a presentation wrapper'),
    ):
        units = {
            'CompositeShrinkageActions': 'FreeStrain/PreStrain dimensionless; Curvature 1/mm; ThermalGradient C/mm; EigenForce N; EigenMoment N mm; Centroid mm. This API already uses the GPC N-mm basis; do not apply the kN/kN m conversion used by section-check reports.',
            'ConcreteShrinkageResult': 'Increment/Total/Drying/Autogenous dimensionless signed strains; NotionalSize mm; Kh dimensionless',
        }.get(name, 'named phase/state selectors and stable case Guids; strains and coefficients dimensionless')
        route('BridGHe.Common.Checks.'+name,fields,target,units,checker)


def empty_type_routes():
    """Types with no declared fields still need a responsibility decision; no default catch-all."""
    routes={}
    def add(prefix,names,target,evidence):
        for name in names.split():routes[prefix+name]={'destination':target,'evidence':evidence,'disposition':'stateless-service-or-adapter'}
    add('BridGHe.Common.Models.','ReinforcementMaterials','Material conversion/default adapter over SteelMaterialEN1992 (Rebar), explicit ultimate strain and declarations','Models/ReinforcementMaterials.cs; ElasticExportContractsTest')
    add('BridGHe.Common.MovingLoads.','BridgeMovingWorkflow MovingObject MovingModelBuilder','Plugin workflow/build adapter over GPC MovingLoadDefinition and Converter Straus7MovingWritePlan; no private physical state remains in these stateless classes','PIANO.md F5/F6; source methods; MovingLoadContractsTest')
    add('BridGHe.Common.Checks.','BridgeCreepCoefficient','Checker BridgeCreepCoefficient.FromModularRatio','Checker/GPCChecker.Test.CompositeBridge/BridgeMigrationPhysicsTests.cs')
    add('BridGHe.Common.Checks.','ConcreteShrinkage','Checker ConcreteShrinkage.Calculate, preserving the frozen formula and applicability range','Checker/GPCChecker.Test.CompositeBridge/BridgeMigrationPhysicsTests.cs')
    add('BridGHe.Common.Checks.','CompositeSectionFactory','GPC ReinforcedConcreteSection.CreateBridgeSection plus source vertical-offset diagnostic; no horizontal/vertical substitution','Model/Sections/Concrete/ReinforcedConcreteSection.cs; BridGHe CompositeSectionFactory.cs')
    add('BridGHe.Common.Checks.','BridgeForceConvention BridgeForceResampling BridgeGpc BridgeMergeValidation BridgePhaseAssembler BridgeResultLabels BridgeSupportReactions BridgeWorkflowBuilder','Plugin extraction/selection/units/presentation adapter over GPC result stores and Checker BridgeWorkflowDefinition/Executor; source methods migrated in F6 without duplicating Checker calculations','PIANO.md F5/F6; source methods; Checker BridgeWorkflowDefinitionTests and BridgeWorkflowExecutionTests')
    add('BridGHe.Common.Geometry.','BeamMomentClassification BridgeAlignmentInputs BridgeBearings BridgeConnections BridgeConstructionSequence BridgeDeckGrillage BridgeDeckMesh BridgeDetailLayout BridgeGeometryTools BridgeLongitudinalBracing BridgeParametricLayout BridgeMemberAssignment BridgeSegmentComposite BridgeSegmentOverride BridgeSlabGrillage BridgeSpanAdvisor BridgeStateOverride','Plugin geometric construction/editing algorithm over registered GPC bridge/member/section/element contracts; Rhino remains plugin-owned','PIANO.md F5/F6; source methods; BridgeDefinitionContractsTest')
    add('TensileLib.FormFinding.','LinearForceDensitySolver','TensileLib solver remains internal; explicit GPC FormFindingDefinition is converted by the plugin adapter','PIANO.md scope; F4-PILOTI.md')
    return routes
