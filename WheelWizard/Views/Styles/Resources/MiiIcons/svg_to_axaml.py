import xml.etree.ElementTree as ET
import os
import argparse
import re

# --- Configuration: Color Mapping ---
COLOR_MAP = {
    # Fill mappings
    "#ffffff": "{StaticResource TemplateColor1}", # white
    "white":   "{StaticResource TemplateColor1}",
    "#5f5f5f": "{StaticResource TemplateColor3}",
    "#808080": "{StaticResource TemplateColor4}",
    "#404040": "{StaticResource TemplateColor5}",

    # Stroke mappings
    "#bfbfbf": "{StaticResource TemplateColor2}",
}
KEEP_UNMAPPED_COLORS = True
DEFAULT_UNMAPPED_BRUSH = "Black"

# Preview Configuration
PREVIEW_NAMESPACE_UTIL = "clr-namespace:WheelWizard.Styles.Util" # ADJUST THIS
MAX_PREVIEW_ITEMS = 79 # Limit how many icons show in the preview (increased slightly)

# --- Helper Functions (normalize_color, get_avalonia_brush_attribute - unchanged) ---
def normalize_color(color_str):
    """Converts color to lowercase hex or name."""
    if not color_str:
        return None
    color_str = color_str.lower().strip()
    if color_str == 'none':
        return 'none'
    if color_str.startswith('#'):
        if len(color_str) == 4:
           color_str = f"#{color_str[1]*2}{color_str[2]*2}{color_str[3]*2}"
        return color_str
    return color_str

def get_avalonia_brush_attribute(svg_color, is_stroke=False):
    """Gets the Avalonia Brush attribute string based on the mapping."""
    normalized = normalize_color(svg_color)
    if not normalized or normalized == 'none':
        return ""

    if normalized in COLOR_MAP:
        resource_key = COLOR_MAP[normalized]
        return f'Brush="{resource_key}"'
    elif KEEP_UNMAPPED_COLORS and normalized.startswith('#'):
         return f'Brush="{normalized.upper()}"'
    elif not KEEP_UNMAPPED_COLORS:
         return f'Brush="{DEFAULT_UNMAPPED_BRUSH}"'
    else:
        print(f"Warning: Unmapped color '{svg_color}' found and cannot be kept directly. Skipping brush.")
        return ""

# --- Core XAML Generation Function (generate_xaml_for_svg - unchanged) ---
def generate_xaml_for_svg(svg_path, output_key):
    """Parses an SVG and generates the Avalonia DrawingImage XAML."""
    try:
        namespaces = {'svg': 'http://www.w3.org/2000/svg'}
        ET.register_namespace('', namespaces['svg'])
        tree = ET.parse(svg_path)
        root = tree.getroot()
    except ET.ParseError as e:
        print(f"Error parsing SVG file {svg_path}: {e}")
        return None
    except FileNotFoundError:
        print(f"Error: SVG file not found at {svg_path}")
        return None

    geometry_drawings = []
    paths = root.findall('.//svg:path', namespaces)
    if not paths:
        paths = root.findall('.//path')
        if not paths:
             print(f"Warning: No <path> elements found in {svg_path}")
             return None

    indent = "            "

    for path in paths:
        path_data = path.get('d')
        if not path_data:
            print(f"Warning: Path in {svg_path} missing 'd' attribute. Skipping.")
            continue

        path_data = re.sub(r'\s+', ' ', path_data).strip()
        path_data_xaml = path_data.replace('"', '"')

        fill_color = path.get('fill')
        stroke_color = path.get('stroke')
        stroke_width = path.get('stroke-width')

        brush_attribute = get_avalonia_brush_attribute(fill_color, is_stroke=False)
        brush_attribute_spaced = f" {brush_attribute}" if brush_attribute else ""

        pen_xaml = ""
        if stroke_color and stroke_color.lower() != 'none' and stroke_width:
            try:
                thickness = float(stroke_width)
                if thickness <= 0: thickness = 1.0
            except (ValueError, TypeError):
                 print(f"Warning: Invalid stroke-width '{stroke_width}' in {svg_path}. Using default 1.")
                 thickness = 1.0

            pen_brush_attribute = get_avalonia_brush_attribute(stroke_color, is_stroke=True)

            if pen_brush_attribute:
                pen_xaml = f"""
{indent}    <GeometryDrawing.Pen>
{indent}        <Pen Thickness="{thickness}" {pen_brush_attribute}/>
{indent}    </GeometryDrawing.Pen>"""

        drawing_xaml = f"""
{indent}<GeometryDrawing{brush_attribute_spaced}
{indent}                 Geometry="{path_data_xaml}">{pen_xaml}
{indent}</GeometryDrawing>"""
        geometry_drawings.append(drawing_xaml)

    if not geometry_drawings:
        return None

    xaml_template = f"""
    <DrawingImage x:Key="{output_key}">
        <DrawingGroup>{"".join(geometry_drawings)}
        </DrawingGroup>
    </DrawingImage>"""
    return xaml_template

# --- Helper Function (sanitize_key - UPDATED) ---
def sanitize_key(filename):
    """Creates a valid PascalCase XAML key from a filename."""
    base = os.path.splitext(filename)[0]
    # Remove invalid characters, replace separators with space for title casing later
    key = re.sub(r'[^\w\s-]', '', base) # Allow word chars, space, hyphen
    key = re.sub(r'[-\s]+', ' ', key).strip() # Replace separators with single space

    # Simple title casing (split by space, capitalize each part)
    parts = key.split(' ')
    key = "".join(part.capitalize() for part in parts)

    # Ensure it doesn't start with a number
    if key and key[0].isdigit():
        key = "_" + key
    # Ensure it's not empty
    if not key:
        key = "UnnamedIcon"
    # Ensure first letter is uppercase (should be covered by capitalize, but good check)
    if key:
         key = key[0].upper() + key[1:]

    return key


# --- Function to generate Preview XAML - UPDATED ---
def generate_preview_xaml(resource_keys):
    """Generates the Design.PreviewWith XAML block."""
    if not resource_keys:
        return ""

    items_indent = "                "
    preview_items = []
    # Take only the first MAX_PREVIEW_ITEMS for the preview
    for key in resource_keys[:MAX_PREVIEW_ITEMS]:
        # Use specific Neutral colors as requested
        item_xaml = f"""
{items_indent}<util:MultiColorExampleComponent IconData="{{DynamicResource {key}}}" IconName="{key}"
{items_indent}                                 Color1="{{StaticResource Neutral50}}"
{items_indent}                                 Color2="{{StaticResource Neutral300}}"
{items_indent}                                 Color3="{{StaticResource Neutral600}}"
{items_indent}                                 Color4="{{StaticResource Neutral800}}"
{items_indent}                                 Color5="{{StaticResource Neutral950}}"
{items_indent}/>""" # Removed Width/Height
        preview_items.append(item_xaml)

    # Removed Width from UniformGrid, Updated Border Background
    preview_xaml = f"""
    <Design.PreviewWith>
        <Border Padding="20" Background="{{StaticResource Neutral400}}">
            <UniformGrid Columns="5">{"".join(preview_items)}
            </UniformGrid>
        </Border>
    </Design.PreviewWith>
"""
    return preview_xaml

# --- Main Execution ---
if __name__ == "__main__":

    input_folder = ""
    while not os.path.isdir(input_folder):
        input_folder = input("Please enter the path to the folder containing SVG files: ").strip()
        if not os.path.isdir(input_folder):
            print(f"Error: '{input_folder}' is not a valid directory. Please try again.")

    folder_name = os.path.basename(os.path.normpath(input_folder))
    parent_dir = os.path.dirname(input_folder)
    output_filename = f"{folder_name}.axaml"
    output_path = os.path.join(parent_dir, output_filename)

    print(f"\nProcessing SVGs in: {input_folder}")
    print(f"Output will be written to: {output_path}")

    all_xaml_outputs = []
    generated_keys = []

    svg_files = [f for f in os.listdir(input_folder) if f.lower().endswith('.svg')]

    if not svg_files:
        print("No SVG files found in the directory.")
        exit(0)

    svg_files.sort()

    for filename in svg_files:
        input_path = os.path.join(input_folder, filename)
        output_key = sanitize_key(filename) # Key is now PascalCase
        print(f"  Processing {filename} -> Key: {output_key}")
        xaml_output = generate_xaml_for_svg(input_path, output_key)
        if xaml_output:
            all_xaml_outputs.append(xaml_output)
            generated_keys.append(output_key)

    if not all_xaml_outputs:
        print("No valid XAML generated.")
        exit(0)

    preview_block = generate_preview_xaml(generated_keys) # Generate updated preview
    joined_icon_xaml = "\n".join(all_xaml_outputs) # Add newline separator

    # Assemble final XAML including namespace, preview, and icons
    final_xaml = f"""<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:util="{PREVIEW_NAMESPACE_UTIL}">

    <!-- Generated by svg_to_avalonia.py from folder: {folder_name} -->
{preview_block}
{joined_icon_xaml}
</ResourceDictionary>
"""

    try:
        output_dir_check = os.path.dirname(output_path)
        if output_dir_check and not os.path.exists(output_dir_check):
             os.makedirs(output_dir_check)

        with open(output_path, 'w', encoding='utf-8') as f:
            f.write(final_xaml)
        print(f"\nSuccessfully wrote Avalonia ResourceDictionary to: {output_path}")
    except IOError as e:
        print(f"\nError writing output file {output_path}: {e}")
    except OSError as e:
         print(f"\nError creating output directory for {output_path}: {e}")