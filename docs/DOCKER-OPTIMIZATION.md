# Docker Image Layer Optimization

The Dockerfile is optimized for minimal download sizes when updating the application.

## Strategy

The image uses a **layered caching strategy** where stable files are copied first and frequently-changing files are copied last. This allows Docker to reuse cached layers for unchanged files.

## Layer Breakdown

### Layer Sizes (from most to least stable)

| Layer | Content | Size | Change Frequency |
|-------|---------|------|------------------|
| 1 | nginx base image | ~41 MB | Never (Alpine Linux) |
| 2 | findutils package | ~461 KB | Never |
| 3 | nginx.conf | ~3 KB | Rarely (config changes) |
| 4 | Static assets (icons, manifest) | ~69 KB | Rarely (branding changes) |
| 5 | CSS files | ~7 KB | Rarely (styling changes) |
| 6 | MudBlazor _content | ~824 KB | Rarely (library updates) |
| 7 | .NET runtime (dotnet.*) | ~5.78 MB | Rarely (.NET updates) |
| 8 | Blazor framework (blazor.*) | ~96 KB | Rarely (.NET updates) |
| 9 | ICU data files | ~4.16 MB | Rarely (.NET updates) |
| 10 | Microsoft.* WASMs | ~1.3 MB | Rarely (framework updates) |
| 11 | System.* WASMs | ~4.64 MB | Rarely (framework updates) |
| 12 | Blazored.* WASMs | ~57 KB | Occasionally (dependency updates) |
| 13 | MudBlazor.* WASMs | ~726 KB | Occasionally (library updates) |
| **14** | **Pianoteq.* WASMs** | **~173 KB** | **EVERY app update** ⚡ |
| 15 | Service worker files | ~19 KB | Every app update |
| 16 | index.html | ~6 KB | Every app update |

### Total Image Size

- **Compressed**: ~18 MB (all layers with gzip/brotli)
- **Uncompressed**: ~60 MB
- **Per-update download**: **~200 KB** (layers 14-16 only)

## How It Works

### Traditional Approach (Before)
```dockerfile
# Everything in one layer
COPY --from=publish /pub/wwwroot /usr/share/nginx/html
```

**Problem**: Any code change forces redownload of entire ~18 MB layer.

### Optimized Approach (After)
```dockerfile
# Layer 1: Stable framework files
COPY --from=publish /pub/wwwroot/_framework/dotnet.* /usr/share/nginx/html/_framework/
COPY --from=publish /pub/wwwroot/_framework/System.*.wasm* /usr/share/nginx/html/_framework/

# Layer 2: Third-party dependencies
COPY --from=publish /pub/wwwroot/_framework/MudBlazor.*.wasm* /usr/share/nginx/html/_framework/

# Layer 3: Application code (changes frequently)
COPY --from=publish /pub/wwwroot/_framework/Pianoteq.*.wasm* /usr/share/nginx/html/_framework/
```

**Benefit**: Only ~200 KB download when app code changes.

## Real-World Impact

### Scenario: Bug Fix in Pianoteq.Client

#### Before Optimization
```
Docker pull:
- Base image layers: 0 MB (cached)
- Application layer: 18 MB (redownload entire wwwroot)
Total download: 18 MB
```

#### After Optimization
```
Docker pull:
- Layers 1-13: 0 MB (cached)
- Layer 14 (Pianoteq WASMs): 173 KB
- Layer 15 (Service worker): 19 KB
- Layer 16 (index.html): 6 KB
Total download: ~200 KB
```

**Download reduction: 99%** 🎉

## Layer Caching Behavior

Docker caches each layer independently:

1. **First deployment**: Downloads all layers (~18 MB)
2. **App update**: Only downloads changed layers (~200 KB)
3. **Dependency update** (e.g., MudBlazor): Downloads layers 13-16 (~1 MB)
4. **.NET update**: Downloads layers 7-16 (~17 MB, but rare)

## Verification

Check layer sizes in your built image:

```bash
docker history pianoteq-pwa:optimized --human | grep COPY
```

Sample output:
```
173kB     COPY /pub/wwwroot/_framework/Pianoteq.*.wasm*       ← App code
726kB     COPY /pub/wwwroot/_framework/MudBlazor.*.wasm*      ← UI library
57.4kB    COPY /pub/wwwroot/_framework/Blazored.*.wasm*       ← LocalStorage
4.64MB    COPY /pub/wwwroot/_framework/System.*.wasm*         ← Framework
1.3MB     COPY /pub/wwwroot/_framework/Microsoft.*.wasm*      ← Framework
```

## Testing the Optimization

### Step 1: Build initial image
```bash
docker build -t pianoteq-pwa:v1 .
```

### Step 2: Make a small code change
Edit [src/Pianoteq.Pwa/Pages/Home.razor](../src/Pianoteq.Pwa/Pages/Home.razor#L1) - change a button text:
```razor
<MudButton>Changed Text</MudButton>
```

### Step 3: Rebuild
```bash
docker build -t pianoteq-pwa:v2 .
```

### Step 4: Observe caching
```
[+] Building 15.3s (27/27) FINISHED
 => CACHED [final  7/16] COPY --from=publish /pub/wwwroot/_framework/dotnet.*
 => CACHED [final  8/16] COPY --from=publish /pub/wwwroot/_framework/blazor.*
 => CACHED [final  9/16] COPY --from=publish /pub/wwwroot/_framework/icudt*
 => CACHED [final 10/16] COPY --from=publish /pub/wwwroot/_framework/Microsoft.*.wasm*
 => CACHED [final 11/16] COPY --from=publish /pub/wwwroot/_framework/System.*.wasm*
 => CACHED [final 12/16] COPY --from=publish /pub/wwwroot/_framework/Blazored.*.wasm*
 => CACHED [final 13/16] COPY --from=publish /pub/wwwroot/_framework/MudBlazor.*.wasm*
 => [final 14/16] COPY --from=publish /pub/wwwroot/_framework/Pianoteq.*.wasm*  ← NEW
 => [final 15/16] COPY --from=publish /pub/wwwroot/service-worker*.js*          ← NEW
```

Layers 7-13 show `CACHED` - no rebuild needed!

## Why WASM Files Change

Blazor .NET 10 uses content-addressed filenames:
- `Pianoteq.Client.ndvr9quqzo.wasm` - hash in filename
- When code changes, hash changes
- Docker sees different filename = new file
- Old layer cached, new layer created

This is intentional and works perfectly with our strategy!

## Best Practices

### 1. Keep layer order stable
Don't rearrange the COPY commands - order matters for caching.

### 2. Group by change frequency
- Static assets first
- Framework files next
- Dependencies after
- App code last

### 3. Use wildcards strategically
```dockerfile
# Good: Matches stable pattern
COPY /pub/wwwroot/_framework/System.*.wasm* /dest/

# Bad: Too broad, includes app code
COPY /pub/wwwroot/_framework/*.wasm /dest/
```

### 4. Include compressed versions
```dockerfile
# Copies .wasm, .wasm.gz, .wasm.br
COPY /pub/wwwroot/_framework/Pianoteq.*.wasm* /dest/
```

The `*` after `.wasm` captures all variants.

## Trade-offs

### Pros
✅ **99% smaller updates** - only changed code
✅ **Faster CI/CD** - build cache hits
✅ **Bandwidth savings** - less data transfer
✅ **Faster deployments** - quicker pulls

### Cons
❌ **More complex Dockerfile** - harder to maintain
❌ **More layers** - slightly larger total image (~5%)
❌ **Needs understanding** - team must know why it's structured this way

## Alternative: Single Layer

If you prefer simplicity over optimization:

```dockerfile
FROM nginx:alpine AS final
COPY nginx.conf /etc/nginx/nginx.conf
COPY --from=publish /pub/wwwroot /usr/share/nginx/html
```

**When to use**: 
- Small team, infrequent deployments
- Bandwidth not a concern
- Simplicity > optimization

**When NOT to use**:
- Frequent deployments (daily/hourly)
- Limited bandwidth
- Pay-per-GB registry costs
- Edge/IoT deployments

## Monitoring

Track layer cache hits in your CI/CD:

```bash
docker build --progress=plain . 2>&1 | grep -c "CACHED"
```

High cache hit ratio = optimization working!

## Future Improvements

### 1. Multi-architecture builds
```dockerfile
FROM --platform=$BUILDPLATFORM nginx:alpine AS final
```
Already supported via build args.

### 2. Separate base image
Create `pianoteq-pwa-base:latest` with layers 1-11:
```dockerfile
FROM pianoteq-pwa-base:latest AS final
COPY --from=publish /pub/wwwroot/_framework/Pianoteq.*.wasm* /dest/
```

Benefits:
- Even faster builds
- Shared base across environments

Drawbacks:
- More images to manage
- Base image updates need coordination

### 3. Content-addressable storage
Some registries (e.g., GitHub Container Registry) deduplicate layers globally:
- Same .NET runtime = shared layer across all .NET apps
- Further storage savings

## References

- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Multi-stage Builds](https://docs.docker.com/build/building/multi-stage/)
- [Layer Caching](https://docs.docker.com/build/cache/)
- [Blazor WASM Deployment](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly)

## Summary

**The optimized Dockerfile reduces update downloads from 18 MB to 200 KB (99% reduction)** by carefully ordering COPY commands from most stable to least stable files. This dramatically improves deployment speed and reduces bandwidth costs.
