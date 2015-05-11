public class T1
{
private TypeMap FindTypeMap(object source, object destination, Type sourceType, Type destinationType, string profileName)
{
	TypeMap typeMap = FindExplicitlyDefinedTypeMap(sourceType, destinationType);

	if (typeMap == null && destinationType.IsNullableType())
	{
		typeMap = FindExplicitlyDefinedTypeMap(sourceType, destinationType.GetTypeOfNullable());
	}

	if (typeMap == null)
	{
		typeMap = _typeMaps.FirstOrDefault(x => x.SourceType == sourceType && x.GetDerivedTypeFor(sourceType) == destinationType);

		if (typeMap == null)
		{
			foreach (var sourceInterface in sourceType.GetInterfaces())
			{
				typeMap = ((IConfigurationProvider)this).FindTypeMapFor(source, destination, sourceInterface, destinationType);

				if (typeMap == null) continue;

				var derivedTypeFor = typeMap.GetDerivedTypeFor(sourceType);
				if (derivedTypeFor != destinationType)
				{
					typeMap = CreateTypeMap(sourceType, derivedTypeFor, profileName, typeMap.ConfiguredMemberList);
				}

				break;
			}

			if ((sourceType.BaseType() != null) && (typeMap == null))
				typeMap = ((IConfigurationProvider)this).FindTypeMapFor(source, destination, sourceType.BaseType(), destinationType);
		}
	}
	if (typeMap == null && sourceType.IsGenericType() && destinationType.IsGenericType())
	{
		var sourceGenericDefinition = sourceType.GetGenericTypeDefinition();
		var destGenericDefinition = destinationType.GetGenericTypeDefinition();
		if (sourceGenericDefinition == null || destGenericDefinition == null)
		{
			return null;
		}
		var genericTypePair = new TypePair(sourceGenericDefinition, destGenericDefinition);
		CreateTypeMapExpression genericTypeMapExpression;

		if (_typeMapExpressionCache.TryGetValue(genericTypePair, out genericTypeMapExpression))
		{
			typeMap = CreateTypeMap(sourceType, destinationType, genericTypeMapExpression.ProfileName,
				genericTypeMapExpression.MemberList);

			var mappingExpression = CreateMappingExpression(typeMap, destinationType);

			genericTypeMapExpression.Accept(mappingExpression);
		}
	}
	return typeMap;
}
}
