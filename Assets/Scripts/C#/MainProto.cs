// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: main_proto.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
/// <summary>Holder for reflection information generated from main_proto.proto</summary>
public static partial class MainProtoReflection {

  #region Descriptor
  /// <summary>File descriptor for main_proto.proto</summary>
  public static pbr::FileDescriptor Descriptor {
    get { return descriptor; }
  }
  private static pbr::FileDescriptor descriptor;

  static MainProtoReflection() {
    byte[] descriptorData = global::System.Convert.FromBase64String(
        string.Concat(
          "ChBtYWluX3Byb3RvLnByb3RvIi8KCEtleXBvaW50Eg0KBWluZGV4GAEgASgJ",
          "EgkKAXgYAiABKAMSCQoBeRgDIAEoA2IGcHJvdG8z"));
    descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
        new pbr::FileDescriptor[] { },
        new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
          new pbr::GeneratedClrTypeInfo(typeof(global::Keypoint), global::Keypoint.Parser, new[]{ "Index", "X", "Y" }, null, null, null, null)
        }));
  }
  #endregion

}
#region Messages
public sealed partial class Keypoint : pb::IMessage<Keypoint>
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    , pb::IBufferMessage
#endif
{
  private static readonly pb::MessageParser<Keypoint> _parser = new pb::MessageParser<Keypoint>(() => new Keypoint());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public static pb::MessageParser<Keypoint> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::MainProtoReflection.Descriptor.MessageTypes[0]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public Keypoint() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public Keypoint(Keypoint other) : this() {
    index_ = other.index_;
    x_ = other.x_;
    y_ = other.y_;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public Keypoint Clone() {
    return new Keypoint(this);
  }

  /// <summary>Field number for the "index" field.</summary>
  public const int IndexFieldNumber = 1;
  private string index_ = "";
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public string Index {
    get { return index_; }
    set {
      index_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  /// <summary>Field number for the "x" field.</summary>
  public const int XFieldNumber = 2;
  private long x_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public long X {
    get { return x_; }
    set {
      x_ = value;
    }
  }

  /// <summary>Field number for the "y" field.</summary>
  public const int YFieldNumber = 3;
  private long y_;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public long Y {
    get { return y_; }
    set {
      y_ = value;
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override bool Equals(object other) {
    return Equals(other as Keypoint);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public bool Equals(Keypoint other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (Index != other.Index) return false;
    if (X != other.X) return false;
    if (Y != other.Y) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override int GetHashCode() {
    int hash = 1;
    if (Index.Length != 0) hash ^= Index.GetHashCode();
    if (X != 0L) hash ^= X.GetHashCode();
    if (Y != 0L) hash ^= Y.GetHashCode();
    if (_unknownFields != null) {
      hash ^= _unknownFields.GetHashCode();
    }
    return hash;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override string ToString() {
    return pb::JsonFormatter.ToDiagnosticString(this);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void WriteTo(pb::CodedOutputStream output) {
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    output.WriteRawMessage(this);
  #else
    if (Index.Length != 0) {
      output.WriteRawTag(10);
      output.WriteString(Index);
    }
    if (X != 0L) {
      output.WriteRawTag(16);
      output.WriteInt64(X);
    }
    if (Y != 0L) {
      output.WriteRawTag(24);
      output.WriteInt64(Y);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  #endif
  }

  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
    if (Index.Length != 0) {
      output.WriteRawTag(10);
      output.WriteString(Index);
    }
    if (X != 0L) {
      output.WriteRawTag(16);
      output.WriteInt64(X);
    }
    if (Y != 0L) {
      output.WriteRawTag(24);
      output.WriteInt64(Y);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(ref output);
    }
  }
  #endif

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public int CalculateSize() {
    int size = 0;
    if (Index.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeStringSize(Index);
    }
    if (X != 0L) {
      size += 1 + pb::CodedOutputStream.ComputeInt64Size(X);
    }
    if (Y != 0L) {
      size += 1 + pb::CodedOutputStream.ComputeInt64Size(Y);
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void MergeFrom(Keypoint other) {
    if (other == null) {
      return;
    }
    if (other.Index.Length != 0) {
      Index = other.Index;
    }
    if (other.X != 0L) {
      X = other.X;
    }
    if (other.Y != 0L) {
      Y = other.Y;
    }
    _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void MergeFrom(pb::CodedInputStream input) {
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    input.ReadRawMessage(this);
  #else
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 10: {
          Index = input.ReadString();
          break;
        }
        case 16: {
          X = input.ReadInt64();
          break;
        }
        case 24: {
          Y = input.ReadInt64();
          break;
        }
      }
    }
  #endif
  }

  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
          break;
        case 10: {
          Index = input.ReadString();
          break;
        }
        case 16: {
          X = input.ReadInt64();
          break;
        }
        case 24: {
          Y = input.ReadInt64();
          break;
        }
      }
    }
  }
  #endif

}

#endregion


#endregion Designer generated code
