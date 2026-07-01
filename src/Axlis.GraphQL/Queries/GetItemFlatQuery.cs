namespace Axlis.GraphQL.Queries;

/// <summary>
/// Contains the GraphQL query for retrieving a Sitecore item without parent, ancestors, or children.
/// More efficient than <see cref="GetItemByPathQuery"/> when tree structure is not needed.
/// </summary>
public static class GetItemFlatQuery
{
    /// <summary>
    /// GraphQL query to retrieve an item by path or ID with all fields.
    /// Does not include parent, ancestors, or children data.
    /// </summary>
    public const string Query = @"
query GetItemFlat($path: String!) {
  item(path: $path, language: ""en"") {
    ...ItemData
  }
}

fragment ItemData on Item {
  ...ItemDetails
}

fragment ItemDetails on Item {
  __typename
  id(format: """")
  name
  displayName
  path
  version
  hasChildren
  template {
    id(format: """")
    name
  }
  fields {
    ... on TextField {
      __typename
      id(format: """")
      name
      value
      jsonValue
    }
    ... on RichTextField {
      __typename
      id(format: """")
      name
      value
      jsonValue
    }
    ... on CheckboxField {
      __typename
      id(format: """")
      name
      value
      boolValue
      jsonValue
    }
    ... on NumberField {
      __typename
      id(format: """")
      name
      value
      numberValue
      jsonValue
    }
    ... on DateField {
      __typename
      id(format: """")
      name
      value
      dateValue
      jsonValue
    }
    ... on ImageField {
      __typename
      id(format: """")
      name
      description
      extension
      keywords
      value
      jsonValue
      height
      width
      size
      src
      title
    }
    ... on LinkField {
      __typename
      id(format: """")
      name
      value
      jsonValue
      linkType
      text
      target
      targetItem {
        ...ItemDetailsDepth2
      }
    }
    ... on MultilistField {
      __typename
      id(format: """")
      name
      count
      value
      targetIds
    }
    ... on LookupField {
      __typename
      id(format: """")
      name
      targetId(format: """")
      value
    }
  }
}

fragment ItemDetailsDepth2 on Item {
  __typename
  id(format: """")
  name
  displayName
  path
  version
  hasChildren
  template {
    id(format: """")
    name
  }
  fields {
    ... on TextField {
      __typename
      id(format: """")
      name
      value
      jsonValue
    }
    ... on RichTextField {
      __typename
      id(format: """")
      name
      value
      jsonValue
    }
    ... on CheckboxField {
      __typename
      id(format: """")
      name
      value
      boolValue
      jsonValue
    }
    ... on NumberField {
      __typename
      id(format: """")
      name
      value
      numberValue
      jsonValue
    }
    ... on DateField {
      __typename
      id(format: """")
      name
      value
      dateValue
      jsonValue
    }
    ... on ImageField {
      __typename
      id(format: """")
      name
      description
      extension
      keywords
      value
      jsonValue
      height
      width
      size
      src
      title
    }
    ... on LinkField {
      __typename
      id(format: """")
      name
      value
      jsonValue
      linkType
      text
      target
    }
    ... on MultilistField {
      __typename
      id(format: """")
      name
      count
      value
      targetIds
    }
    ... on LookupField {
      __typename
      id(format: """")
      name
      targetId(format: """")
      value
    }
  }
}
";
}
