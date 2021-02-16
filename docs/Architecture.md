# Tailviewer Architecture

## Concepts

The following document aims to explain the concepts employed by Tailviewer.

### Descriptors

Tailviewer claims to be extensible, e.g. a custom log file implementation may offer properties which are not known by Tailviewer, and yet still can be displayed to end-users.
The same goes for the columns.  

Descriptors are objects which describe a certain log source property or -column. They do not hold the actual data, they serve as a means to communicate to tailviewer:
- The .NET type which is used to hold values for that property or column
- The default value for that property or column (useful when the default value must differ from `default(T)`)
- A unique identifier for that property and or column (for when Tailviewer needs to persist information about a property or column)
- A way to serialize / deserialize values (where necessary)

#### Property Descriptors

Describes a property of a log source.

#### Column Descriptors

Describes a column of a log source (which is another way of saying a "property" of a log entry).

### Buffers

Buffers are the actual "backing fields" for what descriptors describe: Buffers store the data associated with a property, column or log entry.
Buffers are always backed by memory and provide fast read and write (where applicable) access to their data.

There sometimes exist multiple buffer implementations for the same entity, mostly in the following ways:
- Array: Fixed in size which must be given during construction, intialized to the property's / column's default value
- List: Dynamic in size and may grow / shrink on demand
- Concurrent: Thread-safe view onto another buffer

#### Property Buffers

A Property Buffer stores values of their associated properties. The same property cannot occur more than once.

#### Log Buffers

Log Buffers are the most important ones which are used to store subsets of log entries from their source in memory, so they may be processed. They provide both
row as well as column-based access to their data and allow for various slices to be extracted: It is possible to copy a subset of log entries from one buffer
to another (identified by their index within the buffer) as well as only a subset of columns to another buffer.

Log Buffers store entries in the order they were added / copied into and allow log entries to be accessed again by their insertion index (the first log entry
is at index 0, the next at index 1, etc...).

###  Views

Views are small objects which provide a limited view onto a source. They are similar almost identical to C#'s Span{T} or c++'s string_view.
Views are constructed when one wants to provide read and/or write access to a source buffer without the need for extra copies.

### Log Entries

A log entry is essentially a map from column descriptors to their values. Log entries don't force a partcular storage scheme on their implementation:
This library offers many implementations:
- Log entry one which directly stores values in a dictionary by their column
- An accessor which provides access to the values of a log buffer
- A view which provides access to a subset of properties of an original log entry

### Log Sources

A log source is an accessor to whatever is providing log entries. Most of the time this would be a file on disk, but it can be anything, for example
a socket which receives log entries over the network. Log sources differ from log buffers in that they might be changed (new log entries are added) over
time and that they probably are not backed by main memory (or at least not fully), which is reflected in the interface used to retrieve log entries
from log sources.

Log sources provide access to log entries via their log entry index.

Log sources are expected to notify their listeners when changes are made to the source. Due to the nature of log files these changes are:

#### Append

The most common modification of a log source. Whenver a new log entry (or multiple log entries) are added to the underlying source, then the log source
should forward this event to their listeners so they might inspect the new log entries.

#### Reset

The second most common modification of a log source. It signals that the log source is now completely empty (for example because the log file has been moved, deleted
or has otherwise become unaccessible).

#### Invalidate

The least most common modification of a log source. It signals that a portion of log entries has been removed from the source. Due to the nature of log files,
it is not possible to invalidate a portion on the "middle" of a source. Every invalidate operation is interpreted as every log entry from the first invalid
index to be removed.
This event is necessary for certain edge cases:
- Sorting log entries from multiple source by at the same time will every now and then cause entries to have to be re-ordered, thereby invalidating a short portion of the source
- Reading multi-line log entries which are written to in multiple write operations
