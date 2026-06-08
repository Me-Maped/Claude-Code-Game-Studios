# Godot — Breaking Changes

Last verified: 2026-06-01 | Engine: Godot 4.6.3

Changes between Godot versions, focused on post-LLM-cutoff changes (4.4+).

## 4.5 → 4.6 (Jan 2026 — POST-CUTOFF, HIGH RISK)

| Subsystem | Change | Details |
|-----------|--------|---------|
| Physics | Jolt is now the DEFAULT 3D physics engine | New projects use Jolt automatically. Existing projects keep their setting. Some HingeJoint3D properties (like `damp`) only work with GodotPhysics. |
| Rendering | Glow processes BEFORE tonemapping | Was after tonemapping. Scenes with glow will look different. Adjust intensity/blend in WorldEnvironment. |
| Rendering | D3D12 default on Windows | Was Vulkan. For better driver compatibility. |
| Rendering | AgX tonemapper new controls | White point and contrast parameters added. |
| Core | Quaternion initializes to identity | Was zero. Unlikely to affect most code but technically breaking. |
| UI | Dual-focus system | Mouse/touch focus now separate from keyboard/gamepad focus. Visual feedback differs by input method. |
| Animation | IK system fully restored | CCDIK, FABRIK, Jacobian IK, Spline IK, TwoBoneIK via SkeletonModifier3D nodes. |
| Editor | New "Modern" theme default | Grayscale replaces blue-tint. Restore: Editor Settings → Interface → Theme → Style: Classic |
| Editor | "Select Mode" keybind changed | New "Select Mode" (v key) prevents accidental transforms. Old mode renamed "Transform Mode" (q key). |
| 2D | TileMapLayer scene tile rotation | Scene tiles can now be rotated like atlas tiles. |
| Animation | AnimationPlayer String→StringName | `assigned_animation`, `autoplay`, `current_animation` now StringName. C# binary incompatible. |
| Networking | Methods moved to base classes | StreamPeerTCP→StreamPeerSocket, TCPServer→SocketServer. See deprecated-apis.md. |
| Editor | EditorFileDialog→FileDialog | Many methods/properties moved to base class. C# binary incompatible for some. |
| Localization | CSV plural form support | No longer requires Gettext for plurals. Context columns added. |

### Changed Defaults (4.6)

| Setting | Old | New |
|---------|-----|-----|
| Windows rendering driver (new projects) | Vulkan | D3D12 |
| 3D physics engine (new projects) | Godot Physics | Jolt Physics |
| `Environment.glow_blend_mode` | 2 (Soft Light) | 1 (Screen) |
| `Environment.glow_intensity` | 0.8 | 0.3 |
| `MeshInstance3D.skeleton` | `NodePath("..")` | `NodePath("")` |

### tscn Format Change (4.6)

- `load_steps` no longer written in scene files
- Unique node IDs now saved (helps track nodes across renames/moves)
- Backwards and forwards compatible
- Use `Project > Tools > Upgrade Project Files...` to batch-convert
| C# | Automatic string extraction | Translation strings auto-extracted from C# code. |
| Plugins | New EditorDock class | Specialized container for plugin docks with layout control. |

## 4.4 → 4.5 (Late 2025 — POST-CUTOFF, HIGH RISK)

| Subsystem | Change | Details |
|-----------|--------|---------|
| GDScript | Variadic arguments added | Functions can accept `...` arbitrary params — new language feature |
| GDScript | `@abstract` decorator | Abstract classes and methods now enforceable |
| GDScript | Script backtracing | Detailed call stacks available even in Release builds |
| Rendering | Stencil buffer support | New capability for advanced visual effects |
| Rendering | SMAA 1x antialiasing | New post-processing AA option |
| Rendering | Shader Baker | Pre-compiles shaders — reportedly 20x faster startup on some demos |
| Rendering | Bent normal maps, specular occlusion | New material features |
| Accessibility | Screen reader support | Control nodes work with accessibility tools via AccessKit |
| Editor | Live translation preview | Test GUI layouts in different languages in-editor |
| Physics | 3D interpolation rearchitected | Moved from RenderingServer to SceneTree. API unchanged but internals differ. |
| Animation | BoneConstraint3D | New: AimModifier3D, CopyTransformModifier3D, ConvertTransformModifier3D |
| Resources | `duplicate_deep()` added | New explicit method for deep duplication of nested resources |
| Navigation | Dedicated 2D navigation server | No longer a proxy to 3D navigation; smaller export for 2D games |
| UI | FoldableContainer node | New accordion-style container for collapsible UI sections |
| UI | Recursive Control behavior | Disable mouse/focus interactions across entire node hierarchies |
| Platform | visionOS export support | New platform target |
| Platform | SDL3 gamepad driver | Delegated gamepad handling to SDL library |
| Platform | Android 16KB page support | Required for Google Play targeting Android 15+ |

### CRITICAL: .NET 9 for Android (4.5)

- C# projects exporting to Android now require .NET 9 (Google Play requirement)
- Other platforms continue to use .NET 8 as minimum

### Resource Behavior Change (4.5)

- `Resource.duplicate(true)` (deep copy) no longer duplicates external resources
- Only duplicates resources internal to the resource file
- Use `Resource.duplicate_deep(DEEP_DUPLICATE_ALL)` for old behavior

### Navigation (4.5)

- Region updates now asynchronous by default (threaded)
- Toggle with `navigation/world/region_use_async_iterations`

### Physics (4.5)

- Jolt Physics: `physics/jolt_physics_3d/simulation/areas_detect_static_bodies` setting removed
- Area3D now always detects static bodies — use collision mask/layer to filter

## 4.3 → 4.4 (Mid 2025 — NEAR CUTOFF, VERIFY)

| Subsystem | Change | Details |
|-----------|--------|---------|
| Core | `FileAccess.store_*` return `bool` | Was `void`. Methods: `store_8`, `store_16`, `store_32`, `store_64`, `store_buffer`, `store_csv_line`, `store_double`, `store_float`, `store_half`, `store_line`, `store_pascal_string`, `store_real`, `store_string`, `store_var` |
| Core | `OS.execute_with_pipe` | Added optional `blocking` parameter |
| Core | `RegEx.compile/create_from_string` | Added optional `show_error` parameter |
| Rendering | `RenderingDevice.draw_list_begin` | Many parameters removed; `breadcrumb` parameter added |
| Rendering | Shader texture types | Parameter/return types changed from `Texture2D` to `Texture` |
| Particles | `.restart()` method | Added optional `keep_seed` parameter (CPU/GPU 2D/3D) |
| GUI | `RichTextLabel.push_meta` | Added optional `tooltip` parameter |
| GUI | `GraphEdit.connect_node` | Added optional `keep_alive` parameter |

### Export Annotations (CRITICAL for 4.4)

- **`@export_file`** now stores `uid://` references instead of `res://` paths when assigned from Inspector
- Exported file arrays may contain mixed `uid://` and `res://` paths
- **Workaround (4.4)**: Manually edit `.tscn`/`.tres` files to use `res://` paths
- **Fix (4.5+)**: Use new `@export_file_path` annotation to retain `res://` behavior

### CSG (4.4)

- CSG switched to Manifold library — non-manifold meshes no longer supported
- Use `MeshInstance3D` for non-manifold geometry (quads, planes)

## 4.2 → 4.3 (In Training Data — LOW RISK)

| Subsystem | Change | Details |
|-----------|--------|---------|
| Animation | `Skeleton3D.add_bone` returns `int32` | Was `void` |
| Animation | `bone_pose_updated` signal | Replaced by `skeleton_updated` |
| TileMap | `TileMapLayer` replaces `TileMap` | One node per layer instead of multi-layer single node |
| Navigation | `NavigationRegion2D` | Removed `avoidance_layers`, `constrain_avoidance` properties |
| Editor | `EditorSceneFormatImporterFBX` | Renamed to `EditorSceneFormatImporterFBX2GLTF` |
| Animation | AnimationMixer base class | AnimationPlayer and AnimationTree now extend AnimationMixer |
