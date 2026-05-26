# Create a simple warehouse icon using System.Drawing
Add-Type -AssemblyName System.Drawing

$size = 256
$bitmap = New-Object System.Drawing.Bitmap($size, $size)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)

# Fill background with dark blue
$graphics.Clear([System.Drawing.Color]::FromArgb(15, 23, 42))

# Draw warehouse building shape
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(59, 130, 246))
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(59, 130, 246), 3)

# Main building rectangle
$graphics.FillRectangle($brush, 40, 80, 176, 140)

# Roof triangle
$points = @(
    [System.Drawing.Point]::new(40, 80),
    [System.Drawing.Point]::new(216, 80),
    [System.Drawing.Point]::new(128, 20)
)
$graphics.FillPolygon($brush, $points)

# Door
$doorBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(30, 41, 59))
$graphics.FillRectangle($doorBrush, 100, 160, 56, 60)

# Windows
$windowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(148, 163, 184))
$graphics.FillRectangle($windowBrush, 60, 110, 30, 30)
$graphics.FillRectangle($windowBrush, 100, 110, 30, 30)
$graphics.FillRectangle($windowBrush, 140, 110, 30, 30)
$graphics.FillRectangle($windowBrush, 180, 110, 30, 30)

$graphics.Dispose()

# Convert to ICO format
$bitmap.Save("C:\Users\aizhukov\Desktop\ПРОБА\WarehouseManagement\Assets\app.ico", [System.Drawing.Imaging.ImageFormat]::Icon)
$bitmap.Dispose()

Write-Host "Icon created successfully"
