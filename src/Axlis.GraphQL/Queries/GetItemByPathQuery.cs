namespace Axlis.GraphQL.Queries;

/// <summary>
/// Contains the GraphQL query for retrieving a Sitecore item by path or ID with full tree depth.
/// Fetches the item, its parent chain (2 levels), and first 50 children (1 level each).
/// </summary>
public static class GetItemByPathQuery
{
    /// <summary>
    /// GraphQL query to retrieve an item by path or ID with 2-level recursive depth.
    /// Includes parent chain, first-level children, and field target items.
    /// </summary>
    public const string Query = @"
query GetItemByPath($path: String!) {
  item(path: $path, language: ""en"") {
    ...ItemData
  }
}

fragment ItemData on Item {
  ...ItemDetails
  parent {
    ...ItemDetailsWithDepth
    parent {
      ...ItemDetailsDepth2
    }
  }
  children (first: 50) {
    total
    results {
      ...ItemDetailsWithDepth
    }
  }
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
      #jsonValue
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
      #jsonValue
      targetIds
      targetItems {
        ...ItemDetailsDepth2
      }
    }
    ... on LookupField {
      __typename
      id(format: """")
      name
      targetId(format: """")
      targetItem {
        ...ItemDetailsDepth2
      }
      value
      #jsonValue
    }
  }
}

fragment ItemDetailsWithDepth on Item {
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
      #jsonValue
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
      #jsonValue
      targetIds
      targetItems {
        ...ItemDetailsDepth2
      }
    }
    ... on LookupField {
      __typename
      id(format: """")
      name
      targetId(format: """")
      targetItem {
        ...ItemDetailsDepth2
      }
      value
      #jsonValue
    }
  }
  children (first: 50) {
    total
    results {
      ...ItemDetailsDepth2
    }
  }
  parent {
    ...ItemDetailsDepth2
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
      #jsonValue
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
      #jsonValue
      targetIds
    }
    ... on LookupField {
      __typename
      id(format: """")
      name
      targetId(format: """")
      value
      #jsonValue
    }
  }
}
";
}
