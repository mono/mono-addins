
KEY_FILE=$(top_srcdir)/mono-addins.snk
ASSEMBLY_COMPILER_COMMAND = @MCS@
ASSEMBLY_COMPILER_FLAGS =  -keyfile:$(KEY_FILE) -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG -d:READ_ONLY -d:NET_3_5
ASSEMBLY = ../bin/Mono.Addins.CecilReflector.dll
ASSEMBLY_NAME=Mono.Addins.CecilReflector
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../bin

	
all: $(ASSEMBLY)

FILES =  \
	AssemblyInfo.cs \
	Mono.Addins.CecilReflector/Reflector.cs \
	Mono.Cecil/Mono.Cecil.Cil/Code.cs \
	Mono.Cecil/Mono.Cecil.Cil/CodeReader.cs \
	Mono.Cecil/Mono.Cecil.Cil/CodeWriter.cs \
	Mono.Cecil/Mono.Cecil.Cil/Document.cs \
	Mono.Cecil/Mono.Cecil.Cil/ExceptionHandler.cs \
	Mono.Cecil/Mono.Cecil.Cil/ILProcessor.cs \
	Mono.Cecil/Mono.Cecil.Cil/Instruction.cs \
	Mono.Cecil/Mono.Cecil.Cil/MethodBody.cs \
	Mono.Cecil/Mono.Cecil.Cil/OpCode.cs \
	Mono.Cecil/Mono.Cecil.Cil/OpCodes.cs \
	Mono.Cecil/Mono.Cecil.Cil/SequencePoint.cs \
	Mono.Cecil/Mono.Cecil.Cil/Symbols.cs \
	Mono.Cecil/Mono.Cecil.Cil/VariableDefinition.cs \
	Mono.Cecil/Mono.Cecil.Cil/VariableReference.cs \
	Mono.Cecil/Mono.Cecil.Metadata/BlobHeap.cs \
	Mono.Cecil/Mono.Cecil.Metadata/Buffers.cs \
	Mono.Cecil/Mono.Cecil.Metadata/CodedIndex.cs \
	Mono.Cecil/Mono.Cecil.Metadata/ElementType.cs \
	Mono.Cecil/Mono.Cecil.Metadata/GuidHeap.cs \
	Mono.Cecil/Mono.Cecil.Metadata/Heap.cs \
	Mono.Cecil/Mono.Cecil.Metadata/MetadataToken.cs \
	Mono.Cecil/Mono.Cecil.Metadata/Row.cs \
	Mono.Cecil/Mono.Cecil.Metadata/StringHeap.cs \
	Mono.Cecil/Mono.Cecil.Metadata/TableHeap.cs \
	Mono.Cecil/Mono.Cecil.Metadata/TokenType.cs \
	Mono.Cecil/Mono.Cecil.Metadata/UserStringHeap.cs \
	Mono.Cecil/Mono.Cecil.Metadata/Utilities.cs \
	Mono.Cecil/Mono.Cecil.PE/BinaryStreamReader.cs \
	Mono.Cecil/Mono.Cecil.PE/BinaryStreamWriter.cs \
	Mono.Cecil/Mono.Cecil.PE/ByteBuffer.cs \
	Mono.Cecil/Mono.Cecil.PE/ByteBufferEqualityComparer.cs \
	Mono.Cecil/Mono.Cecil.PE/DataDirectory.cs \
	Mono.Cecil/Mono.Cecil.PE/Image.cs \
	Mono.Cecil/Mono.Cecil.PE/ImageReader.cs \
	Mono.Cecil/Mono.Cecil.PE/ImageWriter.cs \
	Mono.Cecil/Mono.Cecil.PE/Section.cs \
	Mono.Cecil/Mono.Cecil.PE/TextMap.cs \
	Mono.Cecil/Mono.Cecil/ArrayType.cs \
	Mono.Cecil/Mono.Cecil/AssemblyDefinition.cs \
	Mono.Cecil/Mono.Cecil/AssemblyFlags.cs \
	Mono.Cecil/Mono.Cecil/AssemblyHashAlgorithm.cs \
	Mono.Cecil/Mono.Cecil/AssemblyLinkedResource.cs \
	Mono.Cecil/Mono.Cecil/AssemblyNameDefinition.cs \
	Mono.Cecil/Mono.Cecil/AssemblyNameReference.cs \
	Mono.Cecil/Mono.Cecil/AssemblyReader.cs \
	Mono.Cecil/Mono.Cecil/AssemblyWriter.cs \
	Mono.Cecil/Mono.Cecil/BaseAssemblyResolver.cs \
	Mono.Cecil/Mono.Cecil/CallSite.cs \
	Mono.Cecil/Mono.Cecil/CustomAttribute.cs \
	Mono.Cecil/Mono.Cecil/DefaultAssemblyResolver.cs \
	Mono.Cecil/Mono.Cecil/EmbeddedResource.cs \
	Mono.Cecil/Mono.Cecil/EventAttributes.cs \
	Mono.Cecil/Mono.Cecil/EventDefinition.cs \
	Mono.Cecil/Mono.Cecil/EventReference.cs \
	Mono.Cecil/Mono.Cecil/ExportedType.cs \
	Mono.Cecil/Mono.Cecil/FieldAttributes.cs \
	Mono.Cecil/Mono.Cecil/FieldDefinition.cs \
	Mono.Cecil/Mono.Cecil/FieldReference.cs \
	Mono.Cecil/Mono.Cecil/FileAttributes.cs \
	Mono.Cecil/Mono.Cecil/FunctionPointerType.cs \
	Mono.Cecil/Mono.Cecil/GenericInstanceMethod.cs \
	Mono.Cecil/Mono.Cecil/GenericInstanceType.cs \
	Mono.Cecil/Mono.Cecil/GenericParameter.cs \
	Mono.Cecil/Mono.Cecil/GenericParameterAttributes.cs \
	Mono.Cecil/Mono.Cecil/IConstantProvider.cs \
	Mono.Cecil/Mono.Cecil/ICustomAttributeProvider.cs \
	Mono.Cecil/Mono.Cecil/IGenericInstance.cs \
	Mono.Cecil/Mono.Cecil/IGenericParameterProvider.cs \
	Mono.Cecil/Mono.Cecil/IMarshalInfoProvider.cs \
	Mono.Cecil/Mono.Cecil/IMemberDefinition.cs \
	Mono.Cecil/Mono.Cecil/IMetadataScope.cs \
	Mono.Cecil/Mono.Cecil/IMetadataTokenProvider.cs \
	Mono.Cecil/Mono.Cecil/IMethodSignature.cs \
	Mono.Cecil/Mono.Cecil/Import.cs \
	Mono.Cecil/Mono.Cecil/LinkedResource.cs \
	Mono.Cecil/Mono.Cecil/ManifestResourceAttributes.cs \
	Mono.Cecil/Mono.Cecil/MarshalInfo.cs \
	Mono.Cecil/Mono.Cecil/MemberDefinitionCollection.cs \
	Mono.Cecil/Mono.Cecil/MemberReference.cs \
	Mono.Cecil/Mono.Cecil/MetadataResolver.cs \
	Mono.Cecil/Mono.Cecil/MetadataSystem.cs \
	Mono.Cecil/Mono.Cecil/MethodAttributes.cs \
	Mono.Cecil/Mono.Cecil/MethodCallingConvention.cs \
	Mono.Cecil/Mono.Cecil/MethodDefinition.cs \
	Mono.Cecil/Mono.Cecil/MethodImplAttributes.cs \
	Mono.Cecil/Mono.Cecil/MethodReference.cs \
	Mono.Cecil/Mono.Cecil/MethodReturnType.cs \
	Mono.Cecil/Mono.Cecil/MethodSemanticsAttributes.cs \
	Mono.Cecil/Mono.Cecil/MethodSpecification.cs \
	Mono.Cecil/Mono.Cecil/Modifiers.cs \
	Mono.Cecil/Mono.Cecil/ModuleDefinition.cs \
	Mono.Cecil/Mono.Cecil/ModuleKind.cs \
	Mono.Cecil/Mono.Cecil/ModuleReference.cs \
	Mono.Cecil/Mono.Cecil/NativeType.cs \
	Mono.Cecil/Mono.Cecil/ParameterAttributes.cs \
	Mono.Cecil/Mono.Cecil/ParameterDefinition.cs \
	Mono.Cecil/Mono.Cecil/ParameterDefinitionCollection.cs \
	Mono.Cecil/Mono.Cecil/ParameterReference.cs \
	Mono.Cecil/Mono.Cecil/PinnedType.cs \
	Mono.Cecil/Mono.Cecil/PInvokeAttributes.cs \
	Mono.Cecil/Mono.Cecil/PInvokeInfo.cs \
	Mono.Cecil/Mono.Cecil/PointerType.cs \
	Mono.Cecil/Mono.Cecil/PropertyAttributes.cs \
	Mono.Cecil/Mono.Cecil/PropertyDefinition.cs \
	Mono.Cecil/Mono.Cecil/PropertyReference.cs \
	Mono.Cecil/Mono.Cecil/ReferenceType.cs \
	Mono.Cecil/Mono.Cecil/Resource.cs \
	Mono.Cecil/Mono.Cecil/SecurityDeclaration.cs \
	Mono.Cecil/Mono.Cecil/SentinelType.cs \
	Mono.Cecil/Mono.Cecil/TargetRuntime.cs \
	Mono.Cecil/Mono.Cecil/TypeAttributes.cs \
	Mono.Cecil/Mono.Cecil/TypeDefinition.cs \
	Mono.Cecil/Mono.Cecil/TypeDefinitionCollection.cs \
	Mono.Cecil/Mono.Cecil/TypeParser.cs \
	Mono.Cecil/Mono.Cecil/TypeReference.cs \
	Mono.Cecil/Mono.Cecil/TypeSpecification.cs \
	Mono.Cecil/Mono.Cecil/TypeSystem.cs \
	Mono.Cecil/Mono.Cecil/VariantType.cs \
	Mono.Cecil/Mono.Collections.Generic/Collection.cs \
	Mono.Cecil/Mono.Security.Cryptography/CryptoConvert.cs \
	Mono.Cecil/Mono.Security.Cryptography/CryptoService.cs \
	Mono.Cecil/Mono/Actions.cs \
	Mono.Cecil/Mono/Empty.cs \
	Mono.Cecil/Mono/Funcs.cs \
	Mono.Cecil/System.Runtime.CompilerServices/ExtensionAttribute.cs

DATA_FILES =  

RESOURCES = 

EXTRAS = 

REFERENCES =  \
	-r:../bin/Mono.Addins.dll \
	-r:System.Core

DLL_REFERENCES = 

$(ASSEMBLY).mdb: $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(POLICY_ASSEMBLIES)
	mkdir -p $(dir $(ASSEMBLY))		
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$@ -target:$(COMPILE_TARGET) $(build_sources) $(build_resources_embed) $(build_references_ref)

install-data-local: gac-install

uninstall-local: gac-uninstall

include $(top_srcdir)/Makefile.include

CLEANFILES += $(POLICY_ASSEMBLIES) $(POLICY_CONFIGS)
