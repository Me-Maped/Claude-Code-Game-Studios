# Godot Engine — Version Reference

| Field | Value |
|-------|-------|
| **Engine Version** | Godot 4.6.3 |
| **Release Date** | May 20, 2026 |
| **Project Pinned** | 2026-06-01 |
| **Last Docs Verified** | 2026-06-01 |
| **LLM Knowledge Cutoff** | May 2025 |

## Knowledge Gap Warning

The LLM's training data likely covers Godot up to ~4.3. Versions 4.4, 4.5,
and 4.6 introduced significant changes that the model does NOT know about.
Always cross-reference this directory before suggesting Godot API calls.

## Post-Cutoff Version Timeline

| Version | Release | Risk Level | Key Theme |
|---------|---------|------------|-----------|
| 4.4 | Feb 2025 | MEDIUM | Jolt physics option, FileAccess return types, shader texture type changes |
| 4.5 | ~Mid 2025 | HIGH | Accessibility (AccessKit), variadic args, @abstract, shader baker, SMAA |
| 4.6 | Jan 2026 | HIGH | Jolt default, glow rework, D3D12 default on Windows, IK restored |
| 4.6.3 | May 2026 | HIGH | Patch release (bug fixes) |

## Migration Path: 4.3 → 4.6.3

Migrating from 4.3 to 4.6.3 requires reviewing three migration guides:
1. [4.3 → 4.4](https://docs.godotengine.org/en/stable/tutorials/migrating/upgrading_to_godot_4.4.html)
2. [4.4 → 4.5](https://docs.godotengine.org/en/stable/tutorials/migrating/upgrading_to_godot_4.5.html)
3. [4.5 → 4.6](https://docs.godotengine.org/en/stable/tutorials/migrating/upgrading_to_godot_4.6.html)

## Critical Breaking Changes Summary

### 4.4 Breaking Changes
- **FileAccess**: `store_*` methods now return `bool` instead of `void` (C# binary incompatible)
- **@export_file**: Now stores `uid://` references instead of `res://` paths
- **CSG**: Switched to Manifold library — non-manifold meshes no longer supported
- **Shader**: `get_default_texture_parameter` returns `Texture` instead of `Texture2D`
- **RenderingDevice**: `draw_list_begin` parameter signature changed

### 4.5 Breaking Changes
- **.NET 9 required** for Android C# exports
- **JSONRPC**: `set_scope` replaced by `set_method`
- **Node**: `get_rpc_config` renamed to `get_node_rpc_config`
- **Resource.duplicate(true)**: No longer duplicates external resources — use `duplicate_deep(DEEP_DUPLICATE_ALL)`
- **Jolt Physics**: Area3D now always detects static bodies by default
- **Navigation**: Region updates are now asynchronous by default

### 4.6 Breaking Changes
- **AnimationPlayer**: String properties changed to StringName (C# binary incompatible)
- **Networking**: StreamPeerTCP/TCPServer methods moved to base classes
- **EditorFileDialog**: Many methods/properties moved to base class FileDialog
- **Glow**: Default blend mode changed from Soft Light to Screen (significantly brighter)
- **Volumetric Fog**: Blending changed for physical accuracy (appears brighter)
- **Default 3D physics**: Now Jolt Physics for new projects
- **Default Windows renderer**: Now D3D12 for new projects
- **MeshInstance3D**: Default `skeleton` property changed from `NodePath("..")` to `NodePath("")`

## Verified Sources

- Official docs: https://docs.godotengine.org/en/stable/
- 4.3→4.4 migration: https://docs.godotengine.org/en/stable/tutorials/migrating/upgrading_to_godot_4.4.html
- 4.4→4.5 migration: https://docs.godotengine.org/en/stable/tutorials/migrating/upgrading_to_godot_4.5.html
- 4.5→4.6 migration: https://docs.godotengine.org/en/stable/tutorials/migrating/upgrading_to_godot_4.6.html
- Changelog: https://github.com/godotengine/godot/blob/master/CHANGELOG.md
- Release notes: https://godotengine.org/releases/4.6/
