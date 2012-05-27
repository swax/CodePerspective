using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using OpenTK.Graphics.OpenGL;


namespace XLibrary
{
    class FpsCamera
    {
        bool holdingForward;
        bool holdingBackward;
        bool holdingUp;
        bool holdingDown;
        bool holdingLeftStrafe;
        bool holdingRightStrafe;
        bool holdingRun;

        float camXPos;
        float camYPos;
        float camZPos;

        float camXRot;
        float camYRot;

        float movementSpeedFactor = 15.0f;


        public FpsCamera(float xrot, float yrot, float xpos, float ypos, float zpos)
        {
            camXRot = xrot;
            camYRot = yrot;

            camXPos = xpos;
            camYPos = ypos;
            camZPos = zpos;
        }

        internal void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                holdingForward = true;
            else if (e.KeyCode == Keys.S)
                holdingBackward = true;
            else if (e.KeyCode == Keys.A)
                holdingLeftStrafe = true;
            else if (e.KeyCode == Keys.D)
                holdingRightStrafe = true;
            else if (e.KeyData == (Keys.LButton | Keys.ShiftKey | Keys.Control))
                holdingDown = true;
            else if (e.KeyCode == Keys.Space)
                holdingUp = true;
            else if (e.KeyCode == Keys.ShiftKey)
                holdingRun = true;
        }

        internal void KeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                holdingForward = false;
            else if (e.KeyCode == Keys.S)
                holdingBackward = false;
            else if (e.KeyCode == Keys.A)
                holdingLeftStrafe = false;
            else if (e.KeyCode == Keys.D)
                holdingRightStrafe = false;
            else if (e.KeyData == (Keys.LButton | Keys.ShiftKey))
                holdingDown = false;
            else if (e.KeyCode == Keys.Space)
                holdingUp = false;
            else if (e.KeyCode == Keys.ShiftKey)
                holdingRun = false;
        }

        public void MoveTick()
        {
            // Break up our movement into components along the X, Y and Z axis
            double moveX = 0.0f;
            double moveY = 0.0f;
            double moveZ = 0.0f;

            float speed = movementSpeedFactor;
            if (holdingRun)
                speed *= 3;

            if (holdingForward)
            {
                // Control X-Axis movement
                double pitchFactor = Math.Cos(toRads(camXRot));
                moveX += speed * Math.Sin(toRads(camYRot)) * pitchFactor;

                // Control Y-Axis movement
                //moveY += speed * Math.Sin(toRads(camXRot)) * -1.0f;

                // Control Z-Axis movement
                double yawFactor = Math.Cos(toRads(camXRot));
                moveZ += speed * Math.Cos(toRads(camYRot)) * -1.0f * yawFactor;
            }

            if (holdingBackward)
            {
                // Control X-Axis movement
                double pitchFactor = Math.Cos(toRads(camXRot));
                moveX += speed * Math.Sin(toRads(camYRot)) * -1.0f * pitchFactor;

                // Control Y-Axis movement
                moveY += speed * Math.Sin(toRads(camXRot));

                // Control Z-Axis movement
                double yawFactor = Math.Cos(toRads(camXRot));
                moveZ += speed * Math.Cos(toRads(camYRot)) * yawFactor;
            }

            if (holdingUp)
            {
                moveY += speed;
            }

            if (holdingDown)
            {
                moveY -= speed;
            }

            if (holdingLeftStrafe)
            {
                // Calculate our Y-Axis rotation in radians once here because we use it twice
                double yRotRad = toRads(camYRot);

                moveX += -speed * Math.Cos(yRotRad);
                moveZ += -speed * Math.Sin(yRotRad);
            }

            if (holdingRightStrafe)
            {
                // Calculate our Y-Axis rotation in radians once here because we use it twice
                double yRotRad = toRads(camYRot);

                moveX += speed * Math.Cos(yRotRad);
                moveZ += speed * Math.Sin(yRotRad);
            }

            // After combining our movements for any & all keys pressed, assign them to our camera speed along the given axis

            camXPos += (float)moveX;
            camYPos += (float)moveY;
            camZPos += (float)moveZ;
        }

        public void LookTick(Point mousePos, Point centerPos)
        {
            float vertMouseSensitivity = 10.0f;
            float horizMouseSensitivity = 10.0f;

            //cout << "Mouse cursor is at position (" << mouseX << ", " << mouseY << endl;

            float horizMovement = mousePos.X - centerPos.X;
            float vertMovement = mousePos.Y - centerPos.Y;

            camXRot += vertMovement / vertMouseSensitivity;
            camYRot += horizMovement / horizMouseSensitivity;

            // Control looking up and down with the mouse forward/back movement
            // Limit loking up to vertically up
            if (camXRot < -90.0f)
                camXRot = -90.0f;

            // Limit looking down to vertically down
            if (camXRot > 90.0f)
                camXRot = 90.0f;

            // Looking left and right. Keep the angles in the range -180.0f (anticlockwise turn looking behind) to 180.0f (clockwise turn looking behind)
            if (camYRot < -180.0f)
                camYRot += 360.0f;

            if (camYRot > 180.0f)
                camYRot -= 360.0f;
        }

        private double toRads(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        internal void SetupCamera()
        {
            GL.Rotate(camXRot, 1.0f, 0.0f, 0.0f);        // Rotate our camera on the x-axis (looking up and down)
            GL.Rotate(camYRot, 0.0f, 1.0f, 0.0f);        // Rotate our camera on the  y-axis (looking left and right)
            GL.Translate(-camXPos, -camYPos, -camZPos);    // Translate the modelviewm matrix to the position of our camera
        }

        public void DrawHud(int width, int height, Point center, Color crosshair)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, width, height, 0, 0, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GLUtils.SafeDisable(EnableCap.CullFace, () =>
            {
                GL.Clear(ClearBufferMask.DepthBufferBit);

                GLUtils.SafeBegin(BeginMode.Lines, () =>
                {
                    GL.Color3(crosshair);
                    GL.Vertex2(center.X - 2, center.Y);
                    GL.Vertex2(center.X + 3, center.Y);
                    GL.Vertex2(center.X, center.Y - 3);
                    GL.Vertex2(center.X, center.Y + 2);
                });
            });

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
        }
    }
}
